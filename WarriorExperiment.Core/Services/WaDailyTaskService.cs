using Microsoft.EntityFrameworkCore;
using WarriorExperiment.Persistence.Data;
using WarriorExperiment.Persistence.Entities;

namespace WarriorExperiment.Core.Services;

/// <summary>
/// Service for managing daily tasks
/// </summary>
public class WaDailyTaskService
{
    private readonly WaDbContext _context;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="WaDailyTaskService"/> class
    /// </summary>
    /// <param name="context">The database context</param>
    public WaDailyTaskService(WaDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// Gets a daily task by ID
    /// </summary>
    /// <param name="id">The task ID</param>
    /// <returns>The task if found, null otherwise</returns>
    public async Task<WaDailyTask?> GetByIdAsync(int id)
    {
        return await _context.DailyTasks
            .Include(dt => dt.TaskEntries)
            .FirstOrDefaultAsync(dt => dt.Id == id);
    }
    
    /// <summary>
    /// Gets all daily tasks
    /// </summary>
    /// <returns>List of all tasks ordered by sort order then name</returns>
    public async Task<List<WaDailyTask>> GetAllAsync()
    {
        return await _context.DailyTasks
            .OrderBy(dt => dt.SortOrder)
            .ThenBy(dt => dt.Name)
            .ToListAsync();
    }
    
    /// <summary>
    /// Gets all active daily tasks
    /// </summary>
    /// <returns>List of active tasks ordered by sort order then name</returns>
    public async Task<List<WaDailyTask>> GetActiveAsync()
    {
        return await _context.DailyTasks
            .Where(dt => dt.IsActive)
            .OrderBy(dt => dt.SortOrder)
            .ThenBy(dt => dt.Name)
            .ToListAsync();
    }
    
    /// <summary>
    /// Gets paginated daily tasks
    /// </summary>
    /// <param name="pageIndex">Zero-based page index</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paginated list of tasks</returns>
    public async Task<List<WaDailyTask>> GetPagedAsync(int pageIndex, int pageSize)
    {
        return await _context.DailyTasks
            .OrderBy(dt => dt.SortOrder)
            .ThenBy(dt => dt.Name)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    
    /// <summary>
    /// Gets the total count of daily tasks
    /// </summary>
    /// <returns>Total count of tasks</returns>
    public async Task<int> GetCountAsync()
    {
        return await _context.DailyTasks.CountAsync();
    }
    
    /// <summary>
    /// Creates a new daily task
    /// </summary>
    /// <param name="task">The task to create</param>
    /// <returns>The created task with generated ID</returns>
    public async Task<WaDailyTask> CreateAsync(WaDailyTask task)
    {
        // Set sort order to max + 1 if not specified
        if (task.SortOrder == 0)
        {
            var maxSortOrder = await _context.DailyTasks
                .MaxAsync(dt => (int?)dt.SortOrder) ?? 0;
            task.SortOrder = maxSortOrder + 1;
        }
        
        task.CreatedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;
        task.EnteredAt = DateTime.UtcNow;
        
        _context.DailyTasks.Add(task);
        await _context.SaveChangesAsync();
        
        return task;
    }
    
    /// <summary>
    /// Updates an existing daily task
    /// </summary>
    /// <param name="task">The task to update</param>
    /// <returns>The updated task</returns>
    public async Task<WaDailyTask> UpdateAsync(WaDailyTask task)
    {
        task.UpdatedAt = DateTime.UtcNow;
        
        _context.DailyTasks.Update(task);
        await _context.SaveChangesAsync();
        
        return task;
    }
    
    /// <summary>
    /// Deletes a daily task by ID
    /// </summary>
    /// <param name="id">The task ID to delete</param>
    /// <returns>True if deleted successfully, false if not found</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        var task = await _context.DailyTasks.FindAsync(id);
        if (task == null)
            return false;
        
        _context.DailyTasks.Remove(task);
        await _context.SaveChangesAsync();
        
        return true;
    }
    
    /// <summary>
    /// Checks if a task name already exists
    /// </summary>
    /// <param name="name">The task name to check</param>
    /// <param name="excludeTaskId">Optional task ID to exclude from the check (for updates)</param>
    /// <returns>True if name exists, false otherwise</returns>
    public async Task<bool> NameExistsAsync(string name, int? excludeTaskId = null)
    {
        var query = _context.DailyTasks.Where(dt => dt.Name.ToLower() == name.ToLower());
        
        if (excludeTaskId.HasValue)
        {
            query = query.Where(dt => dt.Id != excludeTaskId.Value);
        }
        
        return await query.AnyAsync();
    }
    
    /// <summary>
    /// Reorders tasks by updating their sort order
    /// </summary>
    /// <param name="taskIds">List of task IDs in the desired order</param>
    /// <returns>Task</returns>
    public async Task ReorderTasksAsync(List<int> taskIds)
    {
        var tasks = await _context.DailyTasks
            .Where(dt => taskIds.Contains(dt.Id))
            .ToListAsync();
            
        for (int i = 0; i < taskIds.Count; i++)
        {
            var task = tasks.FirstOrDefault(dt => dt.Id == taskIds[i]);
            if (task != null)
            {
                task.SortOrder = i + 1;
                task.UpdatedAt = DateTime.UtcNow;
            }
        }
        
        await _context.SaveChangesAsync();
    }
}