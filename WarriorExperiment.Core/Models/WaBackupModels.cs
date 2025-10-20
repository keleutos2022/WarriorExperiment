namespace WarriorExperiment.Core.Models;

/// <summary>
/// Represents the result of a backup operation
/// </summary>
public class WaBackupResult
{
    /// <summary>
    /// Gets or sets whether the backup was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the backup file path or identifier
    /// </summary>
    public string? BackupPath { get; set; }

    /// <summary>
    /// Gets or sets the backup file size in bytes
    /// </summary>
    public long? FileSizeBytes { get; set; }

    /// <summary>
    /// Gets or sets when the backup was created
    /// </summary>
    public DateTime BackupDateTime { get; set; }

    /// <summary>
    /// Gets or sets any error message if backup failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets additional metadata about the backup
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Represents the result of a restore operation
/// </summary>
public class WaRestoreResult
{
    /// <summary>
    /// Gets or sets whether the restore was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the number of records restored
    /// </summary>
    public int RecordsRestored { get; set; }

    /// <summary>
    /// Gets or sets any error message if restore failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the restore completion time
    /// </summary>
    public DateTime RestoreDateTime { get; set; }

    /// <summary>
    /// Gets or sets validation results
    /// </summary>
    public List<string> ValidationMessages { get; set; } = new();
}

/// <summary>
/// Configuration options for backup operations
/// </summary>
public class WaBackupOptions
{
    /// <summary>
    /// Gets or sets the backup directory path
    /// </summary>
    public string BackupDirectory { get; set; } = "backups";

    /// <summary>
    /// Gets or sets whether to compress backup files
    /// </summary>
    public bool CompressBackups { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to include schema in database backups
    /// </summary>
    public bool IncludeSchema { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of backup files to retain
    /// </summary>
    public int MaxBackupRetentionCount { get; set; } = 10;

    /// <summary>
    /// Gets or sets the backup file name format
    /// </summary>
    public string BackupFileNameFormat { get; set; } = "wa_backup_{0:yyyyMMdd_HHmmss}";
}

/// <summary>
/// Represents a complete user data export
/// </summary>
public class WaUserDataExport
{
    /// <summary>
    /// Gets or sets the user information
    /// </summary>
    public WaUserExportData User { get; set; } = new();

    /// <summary>
    /// Gets or sets the daily survey entries
    /// </summary>
    public List<WaDailySurveyExportData> DailySurveys { get; set; } = new();

    /// <summary>
    /// Gets or sets the measurement entries
    /// </summary>
    public List<WaMeasurementEntryExportData> MeasurementEntries { get; set; } = new();

    /// <summary>
    /// Gets or sets the rite of passage practice entries
    /// </summary>
    public List<WaRiteOfPassagePracticeExportData> RiteOfPassagePractices { get; set; } = new();

    /// <summary>
    /// Gets or sets the variety practice entries with exercises
    /// </summary>
    public List<WaVarietyPracticeExportData> VarietyPractices { get; set; } = new();

    /// <summary>
    /// Gets or sets the export metadata
    /// </summary>
    public WaExportMetadata Metadata { get; set; } = new();
}

/// <summary>
/// User export data model
/// </summary>
public class WaUserExportData
{
    public string UserName { get; set; } = string.Empty;
    public decimal? Height { get; set; }
    public DateTime? BirthDate { get; set; }
    public DateTime? DateOfStart { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime EnteredAt { get; set; }
}

/// <summary>
/// Daily survey export data model
/// </summary>
public class WaDailySurveyExportData
{
    public DateTime Date { get; set; }
    public int SleepQuality { get; set; }
    public int Energy { get; set; }
    public int Mood { get; set; }
    public int MuscleSoreness { get; set; }
    public string BowelMovement { get; set; } = string.Empty;
    public int StressLevel { get; set; }
    public int HungerFeelingDuringUndereatingPhase { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime EnteredAt { get; set; }
}

/// <summary>
/// Measurement entry export data model
/// </summary>
public class WaMeasurementEntryExportData
{
    public DateTime Date { get; set; }
    public string MeasurementMethodName { get; set; } = string.Empty;
    public decimal? Weight { get; set; }
    public decimal? BodyFat { get; set; }
    public decimal? MuscleMassPercentage { get; set; }
    public decimal? MuscleMass { get; set; }
    public decimal? BMI { get; set; }
    public decimal? ChestCircumference { get; set; }
    public decimal? WaistCircumference { get; set; }
    public decimal? HipCircumference { get; set; }
    public decimal? BicepCircumference { get; set; }
    public decimal? ThighCircumference { get; set; }
    public decimal? CalfCircumference { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime EnteredAt { get; set; }
}

/// <summary>
/// Rite of passage practice export data model
/// </summary>
public class WaRiteOfPassagePracticeExportData
{
    public DateTime Date { get; set; }
    public string PracticeIntensity { get; set; } = string.Empty;
    public int Ladder1Sets { get; set; }
    public int Ladder2Sets { get; set; }
    public int Ladder3Sets { get; set; }
    public int Ladder4Sets { get; set; }
    public int Ladder5Sets { get; set; }
    public int Dice { get; set; }
    public int PullCount { get; set; }
    public decimal? Weight { get; set; }
    public bool Success { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime EnteredAt { get; set; }
}

/// <summary>
/// Variety practice export data model
/// </summary>
public class WaVarietyPracticeExportData
{
    public DateTime Date { get; set; }
    public string? Notes { get; set; }
    public List<WaExerciseExportData> Exercises { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime EnteredAt { get; set; }
}

/// <summary>
/// Exercise export data model
/// </summary>
public class WaExerciseExportData
{
    public string Name { get; set; } = string.Empty;
    public int Sets { get; set; }
    public int Reps { get; set; }
    public decimal? Weight { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime EnteredAt { get; set; }
}

/// <summary>
/// Export metadata
/// </summary>
public class WaExportMetadata
{
    public string ExportVersion { get; set; } = "1.0";
    public DateTime ExportDateTime { get; set; }
    public string ApplicationVersion { get; set; } = string.Empty;
    public string ExportedBy { get; set; } = "System";
    public int TotalRecords { get; set; }
    public Dictionary<string, int> RecordCounts { get; set; } = new();
}