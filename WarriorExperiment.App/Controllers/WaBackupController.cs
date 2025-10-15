using Microsoft.AspNetCore.Mvc;
using WarriorExperiment.Core.Interfaces;
using WarriorExperiment.Core.Models;

namespace WarriorExperiment.App.Controllers;

/// <summary>
/// API controller for backup and restore operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WaBackupController : ControllerBase
{
    private readonly IWaBackupService _backupService;
    private readonly IWaUserDataService _userDataService;
    private readonly ILogger<WaBackupController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="WaBackupController"/> class
    /// </summary>
    /// <param name="backupService">Backup service</param>
    /// <param name="userDataService">User data service</param>
    /// <param name="logger">Logger</param>
    public WaBackupController(IWaBackupService backupService, IWaUserDataService userDataService, ILogger<WaBackupController> logger)
    {
        _backupService = backupService;
        _userDataService = userDataService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a full database backup
    /// </summary>
    /// <param name="backupName">Optional backup name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Backup result</returns>
    [HttpPost("database/backup")]
    public async Task<ActionResult<WaBackupResult>> CreateDatabaseBackup([FromQuery] string? backupName = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _backupService.CreateFullBackupAsync(backupName, cancellationToken);
            
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating database backup");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Restores database from backup
    /// </summary>
    /// <param name="backupPath">Path to backup file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Restore result</returns>
    [HttpPost("database/restore")]
    public async Task<ActionResult<WaRestoreResult>> RestoreDatabase([FromBody] string backupPath, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _backupService.RestoreFromBackupAsync(backupPath, cancellationToken);
            
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring database");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Gets list of available backup files
    /// </summary>
    /// <returns>List of backup file paths</returns>
    [HttpGet("database/backups")]
    public async Task<ActionResult<List<string>>> GetAvailableBackups()
    {
        try
        {
            var backups = await _backupService.GetAvailableBackupsAsync();
            return Ok(backups);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available backups");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Validates a backup file
    /// </summary>
    /// <param name="backupPath">Path to backup file</param>
    /// <returns>Validation result</returns>
    [HttpPost("database/validate")]
    public async Task<ActionResult<bool>> ValidateBackup([FromBody] string backupPath)
    {
        try
        {
            var isValid = await _backupService.ValidateBackupAsync(backupPath);
            return Ok(isValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating backup");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Cleans up old backup files
    /// </summary>
    /// <returns>Number of files deleted</returns>
    [HttpPost("database/cleanup")]
    public async Task<ActionResult<int>> CleanupOldBackups()
    {
        try
        {
            var deletedCount = await _backupService.CleanupOldBackupsAsync();
            return Ok(deletedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up old backups");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Exports data for a specific user
    /// </summary>
    /// <param name="userId">User ID to export</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User data export</returns>
    [HttpGet("users/{userId}/export")]
    public async Task<ActionResult<WaUserDataExport>> ExportUserData(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var export = await _userDataService.ExportUserDataAsync(userId, cancellationToken);
            return Ok(export);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting user data for user {UserId}", userId);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Exports user data to file
    /// </summary>
    /// <param name="userId">User ID to export</param>
    /// <param name="fileName">Optional file name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Export result</returns>
    [HttpPost("users/{userId}/export-file")]
    public async Task<ActionResult<WaBackupResult>> ExportUserDataToFile(int userId, [FromQuery] string? fileName = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _userDataService.ExportUserDataToFileAsync(userId, fileName, cancellationToken);
            
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting user data to file for user {UserId}", userId);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Imports user data from JSON
    /// </summary>
    /// <param name="userDataExport">User data to import</param>
    /// <param name="overwriteExisting">Whether to overwrite existing data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Import result</returns>
    [HttpPost("users/import")]
    public async Task<ActionResult<WaRestoreResult>> ImportUserData([FromBody] WaUserDataExport userDataExport, [FromQuery] bool overwriteExisting = false, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _userDataService.ImportUserDataAsync(userDataExport, overwriteExisting, cancellationToken);
            
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing user data");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Imports user data from file
    /// </summary>
    /// <param name="filePath">Path to the import file</param>
    /// <param name="overwriteExisting">Whether to overwrite existing data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Import result</returns>
    [HttpPost("users/import-file")]
    public async Task<ActionResult<WaRestoreResult>> ImportUserDataFromFile([FromBody] string filePath, [FromQuery] bool overwriteExisting = false, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _userDataService.ImportUserDataFromFileAsync(filePath, overwriteExisting, cancellationToken);
            
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing user data from file");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Gets user data summary
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Export metadata</returns>
    [HttpGet("users/{userId}/summary")]
    public async Task<ActionResult<WaExportMetadata>> GetUserDataSummary(int userId)
    {
        try
        {
            var summary = await _userDataService.GetUserDataSummaryAsync(userId);
            return Ok(summary);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user data summary for user {UserId}", userId);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Validates user data export
    /// </summary>
    /// <param name="userDataExport">User data to validate</param>
    /// <returns>Validation result</returns>
    [HttpPost("users/validate")]
    public async Task<ActionResult<WaRestoreResult>> ValidateUserData([FromBody] WaUserDataExport userDataExport)
    {
        try
        {
            var result = await _userDataService.ValidateUserDataAsync(userDataExport);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating user data");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }
}