using System.Diagnostics;
using System.IO.Compression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WarriorExperiment.Core.Interfaces;
using WarriorExperiment.Core.Models;

namespace WarriorExperiment.Core.Services;

/// <summary>
/// Service for database backup and restore operations using PostgreSQL utilities
/// </summary>
public class WaBackupService : IWaBackupService
{
    private readonly ILogger<WaBackupService> _logger;
    private readonly WaBackupOptions _options;
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="WaBackupService"/> class
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="configuration">Configuration</param>
    /// <param name="options">Backup options (optional)</param>
    public WaBackupService(ILogger<WaBackupService> logger, IConfiguration configuration, IOptions<WaBackupOptions>? options = null)
    {
        _logger = logger;
        _options = options?.Value ?? new WaBackupOptions();
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? 
                           "Host=localhost;Port=7777;Database=warrior_experiment;Username=postgres;Password=postgres";

        // Ensure backup directory exists
        Directory.CreateDirectory(_options.BackupDirectory);
    }

    /// <summary>
    /// Creates a full database backup using pg_dump
    /// </summary>
    /// <param name="backupName">Optional custom backup name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Backup result with details</returns>
    public async Task<WaBackupResult> CreateFullBackupAsync(string? backupName = null, CancellationToken cancellationToken = default)
    {
        var result = new WaBackupResult
        {
            BackupDateTime = DateTime.UtcNow
        };

        try
        {
            _logger.LogInformation("Starting full database backup");

            // Generate backup file name
            var fileName = backupName ?? string.Format(_options.BackupFileNameFormat, DateTime.UtcNow);
            var backupPath = Path.Combine(_options.BackupDirectory, $"{fileName}.sql");

            // Parse connection string for pg_dump
            var connectionParams = ParseConnectionString(_connectionString);

            // Build pg_dump command
            var arguments = BuildPgDumpArguments(connectionParams, backupPath);

            // Execute pg_dump
            var processResult = await ExecuteProcessAsync("pg_dump", arguments, cancellationToken);

            if (processResult.ExitCode == 0)
            {
                // Check if file was created and get size
                if (File.Exists(backupPath))
                {
                    var fileInfo = new FileInfo(backupPath);
                    result.FileSizeBytes = fileInfo.Length;

                    // Compress if enabled
                    if (_options.CompressBackups)
                    {
                        var compressedPath = await CompressBackupFileAsync(backupPath, cancellationToken);
                        if (compressedPath != null)
                        {
                            // Delete original uncompressed file
                            File.Delete(backupPath);
                            backupPath = compressedPath;
                            result.FileSizeBytes = new FileInfo(backupPath).Length;
                        }
                    }

                    result.Success = true;
                    result.BackupPath = backupPath;
                    result.Metadata["RecordsExported"] = await GetDatabaseRecordCountAsync();
                    result.Metadata["CompressedSize"] = result.FileSizeBytes;

                    _logger.LogInformation("Database backup completed successfully: {BackupPath}", backupPath);

                    // Cleanup old backups
                    _ = Task.Run(CleanupOldBackupsAsync, cancellationToken);
                }
                else
                {
                    result.ErrorMessage = "Backup file was not created";
                    _logger.LogError("Backup file was not created: {BackupPath}", backupPath);
                }
            }
            else
            {
                result.ErrorMessage = $"pg_dump failed with exit code {processResult.ExitCode}: {processResult.ErrorOutput}";
                _logger.LogError("pg_dump failed: {Error}", result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Error creating database backup");
        }

        return result;
    }

    /// <summary>
    /// Restores the database from a backup file using psql
    /// </summary>
    /// <param name="backupPath">Path to the backup file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Restore result with details</returns>
    public async Task<WaRestoreResult> RestoreFromBackupAsync(string backupPath, CancellationToken cancellationToken = default)
    {
        var result = new WaRestoreResult
        {
            RestoreDateTime = DateTime.UtcNow
        };

        try
        {
            _logger.LogInformation("Starting database restore from: {BackupPath}", backupPath);

            if (!File.Exists(backupPath))
            {
                result.ErrorMessage = $"Backup file not found: {backupPath}";
                return result;
            }

            // Decompress if needed
            var sqlFilePath = backupPath;
            if (Path.GetExtension(backupPath).Equals(".gz", StringComparison.OrdinalIgnoreCase))
            {
                sqlFilePath = await DecompressBackupFileAsync(backupPath, cancellationToken);
                if (sqlFilePath == null)
                {
                    result.ErrorMessage = "Failed to decompress backup file";
                    return result;
                }
            }

            // Parse connection string for psql
            var connectionParams = ParseConnectionString(_connectionString);

            // Build psql command
            var arguments = BuildPsqlArguments(connectionParams, sqlFilePath);

            // Execute psql
            var processResult = await ExecuteProcessAsync("psql", arguments, cancellationToken);

            if (processResult.ExitCode == 0)
            {
                result.Success = true;
                result.RecordsRestored = await GetDatabaseRecordCountAsync();
                _logger.LogInformation("Database restore completed successfully");
            }
            else
            {
                result.ErrorMessage = $"psql failed with exit code {processResult.ExitCode}: {processResult.ErrorOutput}";
                _logger.LogError("psql failed: {Error}", result.ErrorMessage);
            }

            // Cleanup temporary decompressed file
            if (sqlFilePath != backupPath && File.Exists(sqlFilePath))
            {
                File.Delete(sqlFilePath);
            }
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Error restoring database from backup");
        }

        return result;
    }

    /// <summary>
    /// Lists available backup files
    /// </summary>
    /// <returns>List of backup file paths</returns>
    public async Task<List<string>> GetAvailableBackupsAsync()
    {
        await Task.CompletedTask; // Make async for interface consistency

        if (!Directory.Exists(_options.BackupDirectory))
        {
            return new List<string>();
        }

        var backupFiles = Directory.GetFiles(_options.BackupDirectory, "*.sql")
            .Concat(Directory.GetFiles(_options.BackupDirectory, "*.sql.gz"))
            .OrderByDescending(f => File.GetCreationTime(f))
            .ToList();

        return backupFiles;
    }

    /// <summary>
    /// Deletes old backup files based on retention policy
    /// </summary>
    /// <returns>Number of files deleted</returns>
    public async Task<int> CleanupOldBackupsAsync()
    {
        await Task.CompletedTask; // Make async for interface consistency

        var backupFiles = await GetAvailableBackupsAsync();
        var filesToDelete = backupFiles.Skip(_options.MaxBackupRetentionCount).ToList();

        var deletedCount = 0;
        foreach (var file in filesToDelete)
        {
            try
            {
                File.Delete(file);
                deletedCount++;
                _logger.LogInformation("Deleted old backup file: {File}", file);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete backup file: {File}", file);
            }
        }

        return deletedCount;
    }

    /// <summary>
    /// Validates that a backup file is valid and readable
    /// </summary>
    /// <param name="backupPath">Path to the backup file</param>
    /// <returns>True if backup is valid</returns>
    public async Task<bool> ValidateBackupAsync(string backupPath)
    {
        await Task.CompletedTask; // Make async for interface consistency

        try
        {
            if (!File.Exists(backupPath))
                return false;

            // Basic file size check
            var fileInfo = new FileInfo(backupPath);
            if (fileInfo.Length == 0)
                return false;

            // Check if it's a valid SQL file (basic check)
            var isCompressed = Path.GetExtension(backupPath).Equals(".gz", StringComparison.OrdinalIgnoreCase);
            
            if (isCompressed)
            {
                // For compressed files, try to read the header
                using var fileStream = File.OpenRead(backupPath);
                using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
                var buffer = new byte[1024];
                var bytesRead = await gzipStream.ReadAsync(buffer, 0, buffer.Length);
                return bytesRead > 0;
            }
            else
            {
                // For SQL files, check if it contains SQL-like content
                var firstLine = await File.ReadAllLinesAsync(backupPath).ContinueWith(t => t.Result.FirstOrDefault());
                return !string.IsNullOrEmpty(firstLine) && 
                       (firstLine.Contains("--") || firstLine.Contains("CREATE") || firstLine.Contains("INSERT"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error validating backup file: {BackupPath}", backupPath);
            return false;
        }
    }

    private Dictionary<string, string> ParseConnectionString(string connectionString)
    {
        var parameters = new Dictionary<string, string>();
        var pairs = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);

        foreach (var pair in pairs)
        {
            var keyValue = pair.Split('=', 2);
            if (keyValue.Length == 2)
            {
                parameters[keyValue[0].Trim()] = keyValue[1].Trim();
            }
        }

        return parameters;
    }

    private string BuildPgDumpArguments(Dictionary<string, string> connectionParams, string outputPath)
    {
        var args = new List<string>();

        if (connectionParams.TryGetValue("Host", out var host))
            args.Add($"--host={host}");

        if (connectionParams.TryGetValue("Port", out var port))
            args.Add($"--port={port}");

        if (connectionParams.TryGetValue("Username", out var username))
            args.Add($"--username={username}");

        if (connectionParams.TryGetValue("Database", out var database))
            args.Add($"--dbname={database}");

        // Add common options
        args.Add("--verbose");
        args.Add("--clean");
        args.Add("--create");
        args.Add("--if-exists");
        args.Add($"--file={outputPath}");

        if (!_options.IncludeSchema)
        {
            args.Add("--data-only");
        }

        return string.Join(" ", args);
    }

    private string BuildPsqlArguments(Dictionary<string, string> connectionParams, string inputPath)
    {
        var args = new List<string>();

        if (connectionParams.TryGetValue("Host", out var host))
            args.Add($"--host={host}");

        if (connectionParams.TryGetValue("Port", out var port))
            args.Add($"--port={port}");

        if (connectionParams.TryGetValue("Username", out var username))
            args.Add($"--username={username}");

        if (connectionParams.TryGetValue("Database", out var database))
            args.Add($"--dbname={database}");

        args.Add("--verbose");
        args.Add($"--file={inputPath}");

        return string.Join(" ", args);
    }

    private async Task<ProcessResult> ExecuteProcessAsync(string command, string arguments, CancellationToken cancellationToken)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = command,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        // Set PostgreSQL password from connection string
        var connectionParams = ParseConnectionString(_connectionString);
        if (connectionParams.TryGetValue("Password", out var password))
        {
            processStartInfo.Environment["PGPASSWORD"] = password;
        }

        using var process = new Process { StartInfo = processStartInfo };
        process.Start();

        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync(cancellationToken);

        return new ProcessResult
        {
            ExitCode = process.ExitCode,
            StandardOutput = await outputTask,
            ErrorOutput = await errorTask
        };
    }

    private async Task<string?> CompressBackupFileAsync(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            var compressedPath = $"{filePath}.gz";
            
            using var originalFileStream = File.OpenRead(filePath);
            using var compressedFileStream = File.Create(compressedPath);
            using var compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress);
            
            await originalFileStream.CopyToAsync(compressionStream, cancellationToken);
            
            return compressedPath;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to compress backup file: {FilePath}", filePath);
            return null;
        }
    }

    private async Task<string?> DecompressBackupFileAsync(string compressedPath, CancellationToken cancellationToken)
    {
        try
        {
            var decompressedPath = Path.ChangeExtension(compressedPath, null);
            
            using var compressedFileStream = File.OpenRead(compressedPath);
            using var decompressionStream = new GZipStream(compressedFileStream, CompressionMode.Decompress);
            using var decompressedFileStream = File.Create(decompressedPath);
            
            await decompressionStream.CopyToAsync(decompressedFileStream, cancellationToken);
            
            return decompressedPath;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to decompress backup file: {FilePath}", compressedPath);
            return null;
        }
    }

    private async Task<int> GetDatabaseRecordCountAsync()
    {
        // This would require a database context to count records
        // For now, return a placeholder
        await Task.CompletedTask;
        return 0; // TODO: Implement actual record counting
    }

    private class ProcessResult
    {
        public int ExitCode { get; set; }
        public string StandardOutput { get; set; } = string.Empty;
        public string ErrorOutput { get; set; } = string.Empty;
    }
}