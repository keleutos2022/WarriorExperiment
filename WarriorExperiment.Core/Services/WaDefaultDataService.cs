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
    /// Seeds all default data
    /// </summary>
    /// <returns>Summary of created items</returns>
    public async Task<string> SeedAllDefaultDataAsync()
    {
        var tasksCreated = await SeedDefaultDailyTasksAsync();
        
        return $"Default data seeding completed: {tasksCreated} daily tasks created.";
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
}