using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WarriorExperiment.Core.Interfaces;
using WarriorExperiment.Core.Models;
using WarriorExperiment.Persistence.Data;
using WarriorExperiment.Persistence.Entities;

namespace WarriorExperiment.Core.Services;

/// <summary>
/// Service for user data export and import operations
/// </summary>
public class WaUserDataService : IWaUserDataService
{
    private readonly WaDbContext _context;
    private readonly ILogger<WaUserDataService> _logger;
    private readonly WaBackupOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="WaUserDataService"/> class
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="logger">Logger</param>
    /// <param name="options">Backup options</param>
    public WaUserDataService(WaDbContext context, ILogger<WaUserDataService> logger, IOptions<WaBackupOptions> options)
    {
        _context = context;
        _logger = logger;
        _options = options.Value;

        // Ensure backup directory exists
        Directory.CreateDirectory(_options.BackupDirectory);
    }

    /// <summary>
    /// Exports all data for a specific user
    /// </summary>
    /// <param name="userId">User ID to export</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Complete user data export</returns>
    public async Task<WaUserDataExport> ExportUserDataAsync(int userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting user data export for user ID: {UserId}", userId);

        var user = await _context.Users
            .Include(u => u.DailySurveys)
            .Include(u => u.MeasurementEntries)
                .ThenInclude(me => me.MeasurementMethod)
            .Include(u => u.RiteOfPassagePracticeEntries)
            .Include(u => u.VarietyPractices)
                .ThenInclude(vp => vp.Exercises)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new ArgumentException($"User with ID {userId} not found", nameof(userId));
        }

        var export = new WaUserDataExport
        {
            User = MapUserToExportData(user),
            DailySurveys = user.DailySurveys.Select(MapDailySurveyToExportData).ToList(),
            MeasurementEntries = user.MeasurementEntries.Select(MapMeasurementEntryToExportData).ToList(),
            RiteOfPassagePractices = user.RiteOfPassagePracticeEntries.Select(MapRiteOfPassagePracticeToExportData).ToList(),
            VarietyPractices = user.VarietyPractices.Select(MapVarietyPracticeToExportData).ToList(),
            Metadata = CreateExportMetadata(user)
        };

        _logger.LogInformation("User data export completed for user: {UserName}", user.UserName);
        return export;
    }

    /// <summary>
    /// Imports user data from an export
    /// </summary>
    /// <param name="userDataExport">User data to import</param>
    /// <param name="overwriteExisting">Whether to overwrite existing data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Import result with details</returns>
    public async Task<WaRestoreResult> ImportUserDataAsync(WaUserDataExport userDataExport, bool overwriteExisting = false, CancellationToken cancellationToken = default)
    {
        var result = new WaRestoreResult
        {
            RestoreDateTime = DateTime.UtcNow
        };

        try
        {
            _logger.LogInformation("Starting user data import for user: {UserName}", userDataExport.User.UserName);

            // Validate the data first
            var validationResult = await ValidateUserDataAsync(userDataExport);
            if (!validationResult.Success)
            {
                return validationResult;
            }

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // Check if user already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserName == userDataExport.User.UserName, cancellationToken);

                WaUser user;
                if (existingUser != null)
                {
                    if (!overwriteExisting)
                    {
                        result.ErrorMessage = $"User '{userDataExport.User.UserName}' already exists. Use overwrite option to replace.";
                        return result;
                    }

                    // Remove existing data
                    await RemoveExistingUserDataAsync(existingUser.Id, cancellationToken);
                    user = existingUser;
                    MapExportDataToUser(userDataExport.User, user);
                    _context.Users.Update(user);
                }
                else
                {
                    // Create new user
                    user = new WaUser();
                    MapExportDataToUser(userDataExport.User, user);
                    _context.Users.Add(user);
                }

                await _context.SaveChangesAsync(cancellationToken);

                // Import related data
                result.RecordsRestored += await ImportDailySurveysAsync(user.Id, userDataExport.DailySurveys, cancellationToken);
                result.RecordsRestored += await ImportMeasurementEntriesAsync(user.Id, userDataExport.MeasurementEntries, cancellationToken);
                result.RecordsRestored += await ImportRiteOfPassagePracticesAsync(user.Id, userDataExport.RiteOfPassagePractices, cancellationToken);
                result.RecordsRestored += await ImportVarietyPracticesAsync(user.Id, userDataExport.VarietyPractices, cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                result.Success = true;
                result.RecordsRestored += 1; // For the user record

                _logger.LogInformation("User data import completed successfully for user: {UserName}", userDataExport.User.UserName);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Error importing user data");
        }

        return result;
    }

    /// <summary>
    /// Exports user data to JSON file
    /// </summary>
    /// <param name="userId">User ID to export</param>
    /// <param name="filePath">Target file path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Export result</returns>
    public async Task<WaBackupResult> ExportUserDataToFileAsync(int userId, string? filePath = null, CancellationToken cancellationToken = default)
    {
        var result = new WaBackupResult
        {
            BackupDateTime = DateTime.UtcNow
        };

        try
        {
            var userDataExport = await ExportUserDataAsync(userId, cancellationToken);

            // Generate file name if not provided
            if (string.IsNullOrEmpty(filePath))
            {
                var fileName = $"user_export_{userDataExport.User.UserName}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";
                filePath = Path.Combine(_options.BackupDirectory, fileName);
            }

            // Serialize to JSON
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var jsonString = JsonSerializer.Serialize(userDataExport, jsonOptions);
            await File.WriteAllTextAsync(filePath, jsonString, cancellationToken);

            // Set result properties
            result.Success = true;
            result.BackupPath = filePath;
            result.FileSizeBytes = new FileInfo(filePath).Length;
            result.Metadata["UserName"] = userDataExport.User.UserName;
            result.Metadata["TotalRecords"] = userDataExport.Metadata.TotalRecords;

            _logger.LogInformation("User data exported to file: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Error exporting user data to file");
        }

        return result;
    }

    /// <summary>
    /// Imports user data from JSON file
    /// </summary>
    /// <param name="filePath">Source file path</param>
    /// <param name="overwriteExisting">Whether to overwrite existing data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Import result</returns>
    public async Task<WaRestoreResult> ImportUserDataFromFileAsync(string filePath, bool overwriteExisting = false, CancellationToken cancellationToken = default)
    {
        var result = new WaRestoreResult
        {
            RestoreDateTime = DateTime.UtcNow
        };

        try
        {
            if (!File.Exists(filePath))
            {
                result.ErrorMessage = $"File not found: {filePath}";
                return result;
            }

            var jsonString = await File.ReadAllTextAsync(filePath, cancellationToken);
            
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var userDataExport = JsonSerializer.Deserialize<WaUserDataExport>(jsonString, jsonOptions);
            if (userDataExport == null)
            {
                result.ErrorMessage = "Invalid JSON format in export file";
                return result;
            }

            return await ImportUserDataAsync(userDataExport, overwriteExisting, cancellationToken);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Error importing user data from file: {FilePath}", filePath);
        }

        return result;
    }

    /// <summary>
    /// Gets user data export summary without full data
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Export metadata with record counts</returns>
    public async Task<WaExportMetadata> GetUserDataSummaryAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.DailySurveys)
            .Include(u => u.MeasurementEntries)
            .Include(u => u.RiteOfPassagePracticeEntries)
            .Include(u => u.VarietyPractices)
                .ThenInclude(vp => vp.Exercises)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new ArgumentException($"User with ID {userId} not found", nameof(userId));
        }

        return CreateExportMetadata(user);
    }

    /// <summary>
    /// Validates user data export for consistency
    /// </summary>
    /// <param name="userDataExport">User data to validate</param>
    /// <returns>Validation result</returns>
    public async Task<WaRestoreResult> ValidateUserDataAsync(WaUserDataExport userDataExport)
    {
        await Task.CompletedTask; // Make async for interface consistency

        var result = new WaRestoreResult
        {
            Success = true,
            RestoreDateTime = DateTime.UtcNow
        };

        // Validate user data
        if (string.IsNullOrWhiteSpace(userDataExport.User.UserName))
        {
            result.Success = false;
            result.ValidationMessages.Add("User name is required");
        }

        // Validate data consistency
        var duplicateDates = userDataExport.DailySurveys
            .GroupBy(ds => ds.Date.Date)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateDates.Any())
        {
            result.ValidationMessages.Add($"Duplicate daily survey dates found: {string.Join(", ", duplicateDates)}");
        }

        // Validate measurement methods exist or will be created
        var uniqueMethods = userDataExport.MeasurementEntries
            .Select(me => me.MeasurementMethodName)
            .Distinct()
            .ToList();

        foreach (var methodName in uniqueMethods)
        {
            if (string.IsNullOrWhiteSpace(methodName))
            {
                result.ValidationMessages.Add("Measurement entries contain empty method names");
                break;
            }
        }

        if (result.ValidationMessages.Any())
        {
            result.Success = false;
            result.ErrorMessage = "Validation failed. See validation messages for details.";
        }

        return result;
    }

    #region Private Helper Methods

    private WaUserExportData MapUserToExportData(WaUser user)
    {
        return new WaUserExportData
        {
            UserName = user.UserName,
            Height = user.Height,
            BirthDate = user.BirthDate,
            DateOfStart = user.DateOfStart,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            EnteredAt = user.EnteredAt
        };
    }

    private WaDailySurveyExportData MapDailySurveyToExportData(WaDailySurveyEntry survey)
    {
        return new WaDailySurveyExportData
        {
            Date = survey.Date,
            SleepQuality = survey.SleepQuality,
            Energy = survey.Energy,
            Mood = survey.Mood,
            MuscleSoreness = survey.MuscleSoreness,
            BowelMovement = survey.BowelMovement.ToString(),
            StressLevel = survey.StressLevel,
            HungerFeelingDuringUndereatingPhase = survey.HungerFeelingDuringUndereatingPhase,
            Comment = survey.Comment,
            CreatedAt = survey.CreatedAt,
            UpdatedAt = survey.UpdatedAt,
            EnteredAt = survey.EnteredAt
        };
    }

    private WaMeasurementEntryExportData MapMeasurementEntryToExportData(WaMeasurementEntry entry)
    {
        return new WaMeasurementEntryExportData
        {
            Date = entry.Date,
            MeasurementMethodName = entry.MeasurementMethod?.Name ?? string.Empty,
            Weight = entry.Weight,
            BodyFat = entry.BodyFat,
            MuscleMassPercentage = entry.MuscleMassPercentage,
            MuscleMass = entry.MuscleMass,
            BMI = entry.BMI,
            ChestCircumference = entry.ChestCircumference,
            WaistCircumference = entry.WaistCircumference,
            HipCircumference = entry.HipCircumference,
            BicepCircumference = entry.BicepCircumference,
            ThighCircumference = entry.ThighCircumference,
            CalfCircumference = entry.CalfCircumference,
            Notes = entry.Notes,
            CreatedAt = entry.CreatedAt,
            UpdatedAt = entry.UpdatedAt,
            EnteredAt = entry.EnteredAt
        };
    }

    private WaRiteOfPassagePracticeExportData MapRiteOfPassagePracticeToExportData(WaRiteOfPassagePracticeEntry practice)
    {
        return new WaRiteOfPassagePracticeExportData
        {
            Date = practice.Date,
            PracticeIntensity = practice.PracticeIntensity.ToString(),
            Ladder1Sets = practice.Ladder1Sets,
            Ladder2Sets = practice.Ladder2Sets,
            Ladder3Sets = practice.Ladder3Sets,
            Ladder4Sets = practice.Ladder4Sets,
            Ladder5Sets = practice.Ladder5Sets,
            Dice = practice.Dice,
            PullCount = practice.PullCount,
            Weight = practice.Weight,
            Success = practice.Success,
            CreatedAt = practice.CreatedAt,
            UpdatedAt = practice.UpdatedAt,
            EnteredAt = practice.EnteredAt
        };
    }

    private WaVarietyPracticeExportData MapVarietyPracticeToExportData(WaVarietyPracticeEntry practice)
    {
        return new WaVarietyPracticeExportData
        {
            Date = practice.Date,
            Notes = practice.Notes,
            Exercises = practice.Exercises.Select(MapExerciseToExportData).ToList(),
            CreatedAt = practice.CreatedAt,
            UpdatedAt = practice.UpdatedAt,
            EnteredAt = practice.EnteredAt
        };
    }

    private WaExerciseExportData MapExerciseToExportData(WaExercise exercise)
    {
        return new WaExerciseExportData
        {
            Name = exercise.Name,
            Sets = exercise.Sets,
            Reps = exercise.Reps,
            Weight = exercise.Weight,
            CreatedAt = exercise.CreatedAt,
            UpdatedAt = exercise.UpdatedAt,
            EnteredAt = exercise.EnteredAt
        };
    }

    private WaExportMetadata CreateExportMetadata(WaUser user)
    {
        var exerciseCount = user.VarietyPractices.Sum(vp => vp.Exercises.Count);
        var totalRecords = 1 + user.DailySurveys.Count + user.MeasurementEntries.Count + 
                          user.RiteOfPassagePracticeEntries.Count + user.VarietyPractices.Count + exerciseCount;

        return new WaExportMetadata
        {
            ExportDateTime = DateTime.UtcNow,
            ApplicationVersion = "1.0", // TODO: Get from assembly
            TotalRecords = totalRecords,
            RecordCounts = new Dictionary<string, int>
            {
                ["User"] = 1,
                ["DailySurveys"] = user.DailySurveys.Count,
                ["MeasurementEntries"] = user.MeasurementEntries.Count,
                ["RiteOfPassagePractices"] = user.RiteOfPassagePracticeEntries.Count,
                ["VarietyPractices"] = user.VarietyPractices.Count,
                ["Exercises"] = exerciseCount
            }
        };
    }

    private void MapExportDataToUser(WaUserExportData exportData, WaUser user)
    {
        user.UserName = exportData.UserName;
        user.Height = exportData.Height;
        user.BirthDate = exportData.BirthDate;
        user.DateOfStart = exportData.DateOfStart;
        user.UpdatedAt = DateTime.UtcNow;
        user.EnteredAt = user.EnteredAt == default ? DateTime.UtcNow : user.EnteredAt;
        
        if (user.Id == 0) // New user
        {
            user.CreatedAt = DateTime.UtcNow;
        }
    }

    private async Task RemoveExistingUserDataAsync(int userId, CancellationToken cancellationToken)
    {
        // Remove in correct order to respect foreign key constraints
        var exercises = await _context.Set<WaExercise>()
            .Where(e => e.VarietyPractice!.UserId == userId)
            .ToListAsync(cancellationToken);
        _context.Set<WaExercise>().RemoveRange(exercises);

        var varietyPractices = await _context.Set<WaVarietyPracticeEntry>()
            .Where(vp => vp.UserId == userId)
            .ToListAsync(cancellationToken);
        _context.Set<WaVarietyPracticeEntry>().RemoveRange(varietyPractices);

        var riteOfPassagePractices = await _context.Set<WaRiteOfPassagePracticeEntry>()
            .Where(rp => rp.UserId == userId)
            .ToListAsync(cancellationToken);
        _context.Set<WaRiteOfPassagePracticeEntry>().RemoveRange(riteOfPassagePractices);

        var measurementEntries = await _context.Set<WaMeasurementEntry>()
            .Where(me => me.UserId == userId)
            .ToListAsync(cancellationToken);
        _context.Set<WaMeasurementEntry>().RemoveRange(measurementEntries);

        var dailySurveys = await _context.Set<WaDailySurveyEntry>()
            .Where(ds => ds.UserId == userId)
            .ToListAsync(cancellationToken);
        _context.Set<WaDailySurveyEntry>().RemoveRange(dailySurveys);

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<int> ImportDailySurveysAsync(int userId, List<WaDailySurveyExportData> surveys, CancellationToken cancellationToken)
    {
        foreach (var surveyData in surveys)
        {
            var survey = new WaDailySurveyEntry
            {
                UserId = userId,
                Date = surveyData.Date,
                SleepQuality = surveyData.SleepQuality,
                Energy = surveyData.Energy,
                Mood = surveyData.Mood,
                MuscleSoreness = surveyData.MuscleSoreness,
                BowelMovement = Enum.Parse<WarriorExperiment.Persistence.Enums.WaBowelMovementTime>(surveyData.BowelMovement),
                StressLevel = surveyData.StressLevel,
                HungerFeelingDuringUndereatingPhase = surveyData.HungerFeelingDuringUndereatingPhase,
                Comment = surveyData.Comment,
                CreatedAt = surveyData.CreatedAt,
                UpdatedAt = surveyData.UpdatedAt,
                EnteredAt = surveyData.EnteredAt
            };

            _context.Set<WaDailySurveyEntry>().Add(survey);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return surveys.Count;
    }

    private async Task<int> ImportMeasurementEntriesAsync(int userId, List<WaMeasurementEntryExportData> entries, CancellationToken cancellationToken)
    {
        foreach (var entryData in entries)
        {
            // Get or create measurement method
            var method = await _context.Set<WaMeasurementMethod>()
                .FirstOrDefaultAsync(m => m.Name == entryData.MeasurementMethodName, cancellationToken);

            if (method == null)
            {
                method = new WaMeasurementMethod
                {
                    Name = entryData.MeasurementMethodName,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    EnteredAt = DateTime.UtcNow
                };
                _context.Set<WaMeasurementMethod>().Add(method);
                await _context.SaveChangesAsync(cancellationToken);
            }

            var entry = new WaMeasurementEntry
            {
                UserId = userId,
                MeasurementMethodId = method.Id,
                Date = entryData.Date,
                Weight = entryData.Weight,
                BodyFat = entryData.BodyFat,
                MuscleMassPercentage = entryData.MuscleMassPercentage,
                MuscleMass = entryData.MuscleMass,
                BMI = entryData.BMI,
                ChestCircumference = entryData.ChestCircumference,
                WaistCircumference = entryData.WaistCircumference,
                HipCircumference = entryData.HipCircumference,
                BicepCircumference = entryData.BicepCircumference,
                ThighCircumference = entryData.ThighCircumference,
                CalfCircumference = entryData.CalfCircumference,
                Notes = entryData.Notes,
                CreatedAt = entryData.CreatedAt,
                UpdatedAt = entryData.UpdatedAt,
                EnteredAt = entryData.EnteredAt
            };

            _context.Set<WaMeasurementEntry>().Add(entry);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return entries.Count;
    }

    private async Task<int> ImportRiteOfPassagePracticesAsync(int userId, List<WaRiteOfPassagePracticeExportData> practices, CancellationToken cancellationToken)
    {
        foreach (var practiceData in practices)
        {
            var practice = new WaRiteOfPassagePracticeEntry
            {
                UserId = userId,
                Date = practiceData.Date,
                PracticeIntensity = Enum.Parse<WarriorExperiment.Persistence.Enums.WaPracticeIntensity>(practiceData.PracticeIntensity),
                Ladder1Sets = practiceData.Ladder1Sets,
                Ladder2Sets = practiceData.Ladder2Sets,
                Ladder3Sets = practiceData.Ladder3Sets,
                Ladder4Sets = practiceData.Ladder4Sets,
                Ladder5Sets = practiceData.Ladder5Sets,
                Dice = practiceData.Dice,
                PullCount = practiceData.PullCount,
                Weight = practiceData.Weight,
                Success = practiceData.Success,
                CreatedAt = practiceData.CreatedAt,
                UpdatedAt = practiceData.UpdatedAt,
                EnteredAt = practiceData.EnteredAt
            };

            _context.Set<WaRiteOfPassagePracticeEntry>().Add(practice);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return practices.Count;
    }

    private async Task<int> ImportVarietyPracticesAsync(int userId, List<WaVarietyPracticeExportData> practices, CancellationToken cancellationToken)
    {
        var totalRecords = 0;

        foreach (var practiceData in practices)
        {
            var practice = new WaVarietyPracticeEntry
            {
                UserId = userId,
                Date = practiceData.Date,
                Notes = practiceData.Notes,
                CreatedAt = practiceData.CreatedAt,
                UpdatedAt = practiceData.UpdatedAt,
                EnteredAt = practiceData.EnteredAt
            };

            _context.Set<WaVarietyPracticeEntry>().Add(practice);
            await _context.SaveChangesAsync(cancellationToken);
            totalRecords++;

            // Add exercises
            foreach (var exerciseData in practiceData.Exercises)
            {
                var exercise = new WaExercise
                {
                    VarietyPracticeId = practice.Id,
                    Name = exerciseData.Name,
                    Sets = exerciseData.Sets,
                    Reps = exerciseData.Reps,
                    Weight = exerciseData.Weight,
                    CreatedAt = exerciseData.CreatedAt,
                    UpdatedAt = exerciseData.UpdatedAt,
                    EnteredAt = exerciseData.EnteredAt
                };

                _context.Set<WaExercise>().Add(exercise);
                totalRecords++;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return totalRecords;
    }

    #endregion
}