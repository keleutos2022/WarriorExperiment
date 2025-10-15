using WarriorExperiment.Core.Models;

namespace WarriorExperiment.Core.Interfaces;

/// <summary>
/// Interface for database backup and restore operations
/// </summary>
public interface IWaBackupService
{
    /// <summary>
    /// Creates a full database backup
    /// </summary>
    /// <param name="backupName">Optional custom backup name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Backup result with details</returns>
    Task<WaBackupResult> CreateFullBackupAsync(string? backupName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Restores the database from a backup file
    /// </summary>
    /// <param name="backupPath">Path to the backup file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Restore result with details</returns>
    Task<WaRestoreResult> RestoreFromBackupAsync(string backupPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists available backup files
    /// </summary>
    /// <returns>List of backup file paths</returns>
    Task<List<string>> GetAvailableBackupsAsync();

    /// <summary>
    /// Deletes old backup files based on retention policy
    /// </summary>
    /// <returns>Number of files deleted</returns>
    Task<int> CleanupOldBackupsAsync();

    /// <summary>
    /// Validates that a backup file is valid and readable
    /// </summary>
    /// <param name="backupPath">Path to the backup file</param>
    /// <returns>True if backup is valid</returns>
    Task<bool> ValidateBackupAsync(string backupPath);
}

/// <summary>
/// Interface for user data export and import operations
/// </summary>
public interface IWaUserDataService
{
    /// <summary>
    /// Exports all data for a specific user
    /// </summary>
    /// <param name="userId">User ID to export</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Complete user data export</returns>
    Task<WaUserDataExport> ExportUserDataAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Imports user data from an export file
    /// </summary>
    /// <param name="userDataExport">User data to import</param>
    /// <param name="overwriteExisting">Whether to overwrite existing data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Import result with details</returns>
    Task<WaRestoreResult> ImportUserDataAsync(WaUserDataExport userDataExport, bool overwriteExisting = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports user data to JSON file
    /// </summary>
    /// <param name="userId">User ID to export</param>
    /// <param name="filePath">Target file path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Export result</returns>
    Task<WaBackupResult> ExportUserDataToFileAsync(int userId, string? filePath = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Imports user data from JSON file
    /// </summary>
    /// <param name="filePath">Source file path</param>
    /// <param name="overwriteExisting">Whether to overwrite existing data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Import result</returns>
    Task<WaRestoreResult> ImportUserDataFromFileAsync(string filePath, bool overwriteExisting = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets user data export summary without full data
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Export metadata with record counts</returns>
    Task<WaExportMetadata> GetUserDataSummaryAsync(int userId);

    /// <summary>
    /// Validates user data export for consistency
    /// </summary>
    /// <param name="userDataExport">User data to validate</param>
    /// <returns>Validation result</returns>
    Task<WaRestoreResult> ValidateUserDataAsync(WaUserDataExport userDataExport);
}