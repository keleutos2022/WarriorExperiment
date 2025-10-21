using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WarriorExperiment.Persistence.Data;
using WarriorExperiment.Persistence.Entities;
using WarriorExperiment.Persistence.Enums;

namespace WarriorExperiment.Core.Services;

/// <summary>
/// Service for seeding default data into the application
/// </summary>
public class WaDefaultDataService
{
    private readonly WaDbContext _context;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="WaDefaultDataService"/> class
    /// </summary>
    /// <param name="context">The database context</param>
    public WaDefaultDataService(WaDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// Seeds default daily tasks if they don't already exist
    /// </summary>
    /// <returns>Number of tasks created</returns>
    public async Task<int> SeedDefaultDailyTasksAsync()
    {
        var defaultTasks = new List<(string Name, string Description, WaDaySection PreferredTime, int SortOrder)>
        {
            // Morning tasks
            ("Oil Pulling", "Swish oil in mouth for oral health", WaDaySection.Morning, 1),
            ("Stretching", "Morning body stretches and mobility", WaDaySection.Morning, 2),
            ("Breathwork", "Breathing exercises and techniques", WaDaySection.Morning, 3),
            ("Yoga", "Yoga practice and poses", WaDaySection.Morning, 4),
            ("Pranayama", "Advanced breathing and energy practices", WaDaySection.Morning, 5),
            
            // Other tasks (flexible timing)
            ("Guitar Playing", "Practice guitar and music", WaDaySection.Other, 6),
            ("Intimacy", "Connection and intimate moments", WaDaySection.Other, 7),
            ("Self Love", "Self-care and self-appreciation practices", WaDaySection.Other, 8),
            
            // Evening tasks
            ("Meditation", "Evening meditation and mindfulness", WaDaySection.Evening, 9),
            ("Mouth Trainer", "Oral muscle training and exercises", WaDaySection.Evening, 10)
        };
        
        var createdCount = 0;
        
        foreach (var (name, description, preferredTime, sortOrder) in defaultTasks)
        {
            // Check if task already exists
            var existingTask = await _context.DailyTasks
                .FirstOrDefaultAsync(dt => dt.Name == name);
                
            if (existingTask == null)
            {
                var task = new WaDailyTask
                {
                    Name = name,
                    Description = description,
                    PreferredTime = preferredTime,
                    SortOrder = sortOrder,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    EnteredAt = DateTime.UtcNow
                };
                
                _context.DailyTasks.Add(task);
                createdCount++;
            }
        }
        
        if (createdCount > 0)
        {
            await _context.SaveChangesAsync();
        }
        
        return createdCount;
    }
    
    /// <summary>
    /// Seeds default motivation quotes from embedded JSON file
    /// </summary>
    /// <returns>Number of quotes created</returns>
    public async Task<int> SeedDefaultMotivationQuotesAsync()
    {
        var createdCount = 0;
        
        // Load quotes from embedded resource
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "WarriorExperiment.Core.Data.default-motivation-quotes.json";
        
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            // If embedded resource not found, try loading from file system
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "default-motivation-quotes.json");
            if (File.Exists(filePath))
            {
                var jsonContent = await File.ReadAllTextAsync(filePath);
                var quotes = JsonSerializer.Deserialize<List<MotivationQuoteData>>(jsonContent);
                
                if (quotes != null)
                {
                    foreach (var quoteData in quotes)
                    {
                        // Check if quote already exists
                        var existingQuote = await _context.MotivationQuotes
                            .FirstOrDefaultAsync(mq => mq.Quote == quoteData.Quote && mq.Author == quoteData.Author);
                            
                        if (existingQuote == null)
                        {
                            var quote = new WaMotivationQuote
                            {
                                Author = quoteData.Author,
                                Quote = quoteData.Quote,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow,
                                EnteredAt = DateTime.UtcNow
                            };
                            
                            _context.MotivationQuotes.Add(quote);
                            createdCount++;
                        }
                    }
                }
            }
        }
        else
        {
            using var reader = new StreamReader(stream);
            var jsonContent = await reader.ReadToEndAsync();
            var quotes = JsonSerializer.Deserialize<List<MotivationQuoteData>>(jsonContent);
            
            if (quotes != null)
            {
                foreach (var quoteData in quotes)
                {
                    // Check if quote already exists
                    var existingQuote = await _context.MotivationQuotes
                        .FirstOrDefaultAsync(mq => mq.Quote == quoteData.Quote && mq.Author == quoteData.Author);
                        
                    if (existingQuote == null)
                    {
                        var quote = new WaMotivationQuote
                        {
                            Author = quoteData.Author,
                            Quote = quoteData.Quote,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            EnteredAt = DateTime.UtcNow
                        };
                        
                        _context.MotivationQuotes.Add(quote);
                        createdCount++;
                    }
                }
            }
        }
        
        if (createdCount > 0)
        {
            await _context.SaveChangesAsync();
        }
        
        return createdCount;
    }
    
    /// <summary>
    /// Seeds all default data
    /// </summary>
    /// <returns>Summary of created items</returns>
    public async Task<string> SeedAllDefaultDataAsync()
    {
        var tasksCreated = await SeedDefaultDailyTasksAsync();
        var quotesCreated = await SeedDefaultMotivationQuotesAsync();
        
        return $"Default data seeding completed: {tasksCreated} daily tasks created, {quotesCreated} motivation quotes created.";
    }
    
    /// <summary>
    /// Checks if default daily tasks have been seeded
    /// </summary>
    /// <returns>True if default tasks exist</returns>
    public async Task<bool> HasDefaultDailyTasksAsync()
    {
        var taskCount = await _context.DailyTasks.CountAsync();
        return taskCount >= 10; // We have 10 default tasks
    }
    
    /// <summary>
    /// Internal class for deserializing motivation quote data from JSON
    /// </summary>
    private class MotivationQuoteData
    {
        public string Author { get; set; } = string.Empty;
        public string Quote { get; set; } = string.Empty;
    }
}