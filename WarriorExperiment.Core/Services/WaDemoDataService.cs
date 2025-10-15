using Microsoft.EntityFrameworkCore;
using WarriorExperiment.Persistence.Data;
using WarriorExperiment.Persistence.Models;
using WarriorExperiment.Persistence.Enums;

namespace WarriorExperiment.Core.Services;

/// <summary>
/// Service for generating demo data for the Warrior Experiment application
/// </summary>
public class WaDemoDataService
{
    private readonly WaDbContext _context;
    private readonly WaUserService _userService;
    private readonly WaMeasurementEntryService _measurementService;
    private readonly WaDailySurveyEntryService _surveyService;
    private readonly WaRiteOfPassagePracticeEntryService _practiceService;
    private readonly Random _random;

    /// <summary>
    /// Initializes a new instance of the <see cref="WaDemoDataService"/> class
    /// </summary>
    public WaDemoDataService(
        WaDbContext context,
        WaUserService userService,
        WaMeasurementEntryService measurementService,
        WaDailySurveyEntryService surveyService,
        WaRiteOfPassagePracticeEntryService practiceService)
    {
        _context = context;
        _userService = userService;
        _measurementService = measurementService;
        _surveyService = surveyService;
        _practiceService = practiceService;
        _random = new Random();
    }

    /// <summary>
    /// Generates all demo data including user and 8 weeks of entries
    /// </summary>
    /// <returns>The created demo user</returns>
    public async Task<WaUser> GenerateAllDemoDataAsync()
    {
        // Create demo user
        var demoUser = await CreateDemoUserAsync();
        
        // Ensure measurement method exists
        await EnsureMeasurementMethodExistsAsync();
        
        // Generate data for 8 weeks
        var startDate = DateTime.Now.AddDays(-56); // 8 weeks ago
        
        // Generate weekly measurement entries (8 total)
        await GenerateWeeklyMeasurementEntriesAsync(demoUser.Id, startDate);
        
        // Generate daily survey entries (56 total)
        await GenerateDailySurveyEntriesAsync(demoUser.Id, startDate);
        
        // Generate practice entries (3 per week, 24 total)
        await GeneratePracticeEntriesAsync(demoUser.Id, startDate);
        
        return demoUser;
    }

    /// <summary>
    /// Creates a demo user with realistic profile data
    /// </summary>
    private async Task<WaUser> CreateDemoUserAsync()
    {
        var demoUser = new WaUser
        {
            UserName = "DemoWarrior",
            Height = 175.0m, // 175 cm
            BirthDate = new DateTime(1990, 5, 15),
            DateOfStart = DateTime.Now.AddDays(-56), // Started 8 weeks ago
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            EnteredAt = DateTime.UtcNow
        };

        _context.Users.Add(demoUser);
        await _context.SaveChangesAsync();
        
        return demoUser;
    }

    /// <summary>
    /// Ensures a default measurement method exists
    /// </summary>
    private async Task EnsureMeasurementMethodExistsAsync()
    {
        var existingMethod = await _context.MeasurementMethods.FirstOrDefaultAsync();
        if (existingMethod == null)
        {
            var method = new WaMeasurementMethod
            {
                Name = "TANITA Body Composition Scale",
                Description = "Professional body composition scale for accurate measurements",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                EnteredAt = DateTime.UtcNow
            };
            
            _context.MeasurementMethods.Add(method);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Generates demo Base64 images for measurement entries
    /// </summary>
    private (string front, string back, string side) GenerateDemoImages()
    {
        // Create simple colored rectangle placeholders as Base64 strings
        // These are minimal 1x1 pixel images in different colors
        var frontPhoto = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChAI9RYYDLwAAAABJRU5ErkJggg=="; // Blue
        var backPhoto = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg=="; // Green
        var sidePhoto = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8/5+hHgAHggJ/PchI7wAAAABJRU5ErkJggg=="; // Red
        
        return (frontPhoto, backPhoto, sidePhoto);
    }

    /// <summary>
    /// Generates weekly measurement entries for 8 weeks
    /// </summary>
    private async Task GenerateWeeklyMeasurementEntriesAsync(int userId, DateTime startDate)
    {
        var measurementMethod = await _context.MeasurementMethods.FirstAsync();
        
        // Starting measurements
        decimal startWeight = 80.0m;
        decimal startBodyFat = 15.0m;
        decimal startMuscleMass = 65.0m;
        
        for (int week = 0; week < 8; week++)
        {
            var measurementDate = startDate.AddDays(week * 7);
            var (frontPhoto, backPhoto, sidePhoto) = GenerateDemoImages();
            
            // Progressive improvements over time
            var weightChange = -0.3m * week; // Lose 0.3kg per week
            var bodyFatChange = -0.2m * week; // Lose 0.2% body fat per week
            var muscleMassChange = 0.1m * week; // Gain 0.1kg muscle per week
            
            var entry = new WaMeasurementEntry
            {
                UserId = userId,
                MeasurementMethodId = measurementMethod.Id,
                Date = measurementDate,
                FrontPhoto = frontPhoto,
                BackPhoto = backPhoto,
                SidePhoto = sidePhoto,
                Weight = startWeight + weightChange + (decimal)(_random.NextDouble() * 0.4 - 0.2),
                BodyFat = startBodyFat + bodyFatChange + (decimal)(_random.NextDouble() * 0.4 - 0.2),
                MuscleMass = startMuscleMass + muscleMassChange + (decimal)(_random.NextDouble() * 0.2 - 0.1),
                WaterPercentage = 60.0m + (decimal)(_random.NextDouble() * 4 - 2),
                BoneMass = 3.2m + (decimal)(_random.NextDouble() * 0.2 - 0.1),
                BMI = 26.0m - 0.1m * week + (decimal)(_random.NextDouble() * 0.2 - 0.1),
                VisceralFat = 8 - week / 2 + _random.Next(-1, 2),
                MetabolicAge = 35 - week + _random.Next(-2, 3),
                BasalMetabolicRate = 1800 + week * 5 + _random.Next(-20, 21),
                ChestCircumference = 100.0m + 0.2m * week + (decimal)(_random.NextDouble() * 1 - 0.5),
                WaistCircumference = 85.0m - 0.3m * week + (decimal)(_random.NextDouble() * 1 - 0.5),
                HipCircumference = 95.0m + 0.1m * week + (decimal)(_random.NextDouble() * 1 - 0.5),
                BicepCircumference = 35.0m + 0.1m * week + (decimal)(_random.NextDouble() * 0.5 - 0.25),
                ThighCircumference = 55.0m + 0.2m * week + (decimal)(_random.NextDouble() * 1 - 0.5),
                CalfCircumference = 38.0m + 0.1m * week + (decimal)(_random.NextDouble() * 0.5 - 0.25),
                Notes = week == 0 ? "Starting measurements" : 
                       week == 7 ? "Final measurements - great progress!" : 
                       $"Week {week + 1} measurements",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                EnteredAt = DateTime.UtcNow
            };

            _context.MeasurementEntries.Add(entry);
        }
        
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Generates daily survey entries for 56 days (8 weeks)
    /// </summary>
    private async Task GenerateDailySurveyEntriesAsync(int userId, DateTime startDate)
    {
        var bowelMovementTimes = Enum.GetValues<WaBowelMovementTime>();
        
        for (int day = 0; day < 56; day++)
        {
            var surveyDate = startDate.AddDays(day);
            
            // Generate realistic ratings with some variation
            var sleepQuality = GenerateRating(6, 9); // Generally good sleep
            var energy = GenerateRating(5, 8); // Moderate to high energy
            var mood = GenerateRating(6, 9); // Generally positive mood
            var muscleSoreness = day % 3 == 0 ? GenerateRating(6, 9) : GenerateRating(2, 5); // Higher after workout days
            var stressLevel = GenerateRating(2, 6); // Low to moderate stress
            var hungerFeeling = GenerateRating(3, 7); // Moderate hunger during undereating
            
            var entry = new WaDailySurveyEntry
            {
                UserId = userId,
                Date = surveyDate,
                SleepQuality = sleepQuality,
                Energy = energy,
                Mood = mood,
                MuscleSoreness = muscleSoreness,
                BowelMovement = bowelMovementTimes[_random.Next(bowelMovementTimes.Length)],
                StressLevel = stressLevel,
                HungerFeelingDuringUndereatingPhase = hungerFeeling,
                Comment = day % 7 == 0 ? "Starting new week strong!" :
                         day % 7 == 6 ? "Week completed, feeling good" :
                         _random.Next(0, 10) == 0 ? "Had a great day today" : null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                EnteredAt = DateTime.UtcNow
            };

            _context.DailySurveys.Add(entry);
        }
        
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Generates practice entries (3 per week, 24 total) with proper intensity rotation
    /// </summary>
    private async Task GeneratePracticeEntriesAsync(int userId, DateTime startDate)
    {
        var intensities = new[] { WaPracticeIntensity.Heavy, WaPracticeIntensity.Light, WaPracticeIntensity.Medium };
        int intensityIndex = 0;
        
        WaRiteOfPassagePracticeEntry? lastHeavyEntry = null;
        
        for (int week = 0; week < 8; week++)
        {
            // 3 practices per week: Monday, Wednesday, Friday
            var weekStart = startDate.AddDays(week * 7);
            var practiceDates = new[]
            {
                weekStart, // Monday
                weekStart.AddDays(2), // Wednesday  
                weekStart.AddDays(4)  // Friday
            };

            foreach (var practiceDate in practiceDates)
            {
                var intensity = intensities[intensityIndex % 3];
                intensityIndex++;
                
                // Use the service to calculate proper ladder values
                var (ladder1, ladder2, ladder3, ladder4, ladder5) = _practiceService.CalculateSuggestedLadderValues(lastHeavyEntry, intensity);
                
                var dice = _random.Next(2, 13); // 2-12 dice roll
                var pullCount = CalculatePullCount(ladder1, ladder2, ladder3, ladder4, ladder5);
                var weight = intensity == WaPracticeIntensity.Heavy ? 24m : 
                           intensity == WaPracticeIntensity.Medium ? 16m : 8m;
                
                // Success rate improves over time
                var successRate = intensity == WaPracticeIntensity.Heavy ? 0.7 + (week * 0.04) :
                                intensity == WaPracticeIntensity.Medium ? 0.8 + (week * 0.03) :
                                0.9 + (week * 0.01);
                var success = _random.NextDouble() < successRate;

                var entry = new WaRiteOfPassagePracticeEntry
                {
                    UserId = userId,
                    Date = practiceDate,
                    PracticeIntensity = intensity,
                    Ladder1Sets = ladder1,
                    Ladder2Sets = ladder2,
                    Ladder3Sets = ladder3,
                    Ladder4Sets = ladder4,
                    Ladder5Sets = ladder5,
                    Dice = dice,
                    PullCount = pullCount,
                    Weight = weight,
                    Success = success
                };

                // Use the service to create the entry properly
                var createdEntry = await _practiceService.CreateAsync(entry);
                
                // Track the last successful heavy entry for progression
                if (intensity == WaPracticeIntensity.Heavy && success)
                {
                    lastHeavyEntry = createdEntry;
                }
            }
        }
    }


    /// <summary>
    /// Calculates total pull count from ladder values
    /// </summary>
    private int CalculatePullCount(int l1, int l2, int l3, int l4, int l5)
    {
        int total = 0;
        
        // Each ladder contributes: 1 + 2 + ... + n = n*(n+1)/2
        if (l1 > 0) total += l1 * (l1 + 1) / 2;
        if (l2 > 0) total += l2 * (l2 + 1) / 2;
        if (l3 > 0) total += l3 * (l3 + 1) / 2;
        if (l4 > 0) total += l4 * (l4 + 1) / 2;
        if (l5 > 0) total += l5 * (l5 + 1) / 2;
        
        return total;
    }

    /// <summary>
    /// Generates a rating between min and max with some randomization
    /// </summary>
    private int GenerateRating(int min, int max)
    {
        return _random.Next(min, max + 1);
    }

    /// <summary>
    /// Clears all demo data for the demo user
    /// </summary>
    public async Task ClearDemoDataAsync()
    {
        var demoUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "DemoWarrior");
        if (demoUser != null)
        {
            _context.Users.Remove(demoUser);
            await _context.SaveChangesAsync();
        }
    }
}