using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WarriorExperiment.Persistence.Data;
using WarriorExperiment.Persistence.Entities;

namespace WarriorExperiment.Core.Services;

/// <summary>
/// Service for managing motivation quotes
/// </summary>
public class WaMotivationQuoteService
{
    private readonly WaDbContext _context;
    private readonly ILogger<WaMotivationQuoteService> _logger;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="WaMotivationQuoteService"/> class
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="logger">The logger</param>
    public WaMotivationQuoteService(WaDbContext context, ILogger<WaMotivationQuoteService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    /// <summary>
    /// Gets all motivation quotes
    /// </summary>
    /// <returns>List of all quotes</returns>
    public async Task<List<WaMotivationQuote>> GetAllAsync()
    {
        return await _context.MotivationQuotes
            .OrderBy(q => q.Author)
            .ThenBy(q => q.Id)
            .ToListAsync();
    }
    
    /// <summary>
    /// Gets a motivation quote by ID
    /// </summary>
    /// <param name="id">The quote ID</param>
    /// <returns>The quote or null if not found</returns>
    public async Task<WaMotivationQuote?> GetByIdAsync(int id)
    {
        return await _context.MotivationQuotes
            .FirstOrDefaultAsync(q => q.Id == id);
    }
    
    /// <summary>
    /// Gets a random motivation quote
    /// </summary>
    /// <returns>A random quote or null if no quotes exist</returns>
    public async Task<WaMotivationQuote?> GetRandomQuoteAsync()
    {
        var count = await _context.MotivationQuotes.CountAsync();
        if (count == 0)
            return null;
            
        var randomIndex = new Random().Next(0, count);
        return await _context.MotivationQuotes
            .OrderBy(q => q.Id)
            .Skip(randomIndex)
            .FirstOrDefaultAsync();
    }
    
    /// <summary>
    /// Creates a new motivation quote
    /// </summary>
    /// <param name="quote">The quote to create</param>
    /// <returns>The created quote</returns>
    public async Task<WaMotivationQuote> CreateAsync(WaMotivationQuote quote)
    {
        quote.CreatedAt = DateTime.UtcNow;
        quote.UpdatedAt = DateTime.UtcNow;
        quote.EnteredAt = DateTime.UtcNow;
        
        _context.MotivationQuotes.Add(quote);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Created motivation quote {Id} by {Author}", quote.Id, quote.Author);
        return quote;
    }
    
    /// <summary>
    /// Updates an existing motivation quote
    /// </summary>
    /// <param name="quote">The quote to update</param>
    /// <returns>The updated quote</returns>
    public async Task<WaMotivationQuote> UpdateAsync(WaMotivationQuote quote)
    {
        quote.UpdatedAt = DateTime.UtcNow;
        
        _context.MotivationQuotes.Update(quote);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Updated motivation quote {Id}", quote.Id);
        return quote;
    }
    
    /// <summary>
    /// Deletes a motivation quote
    /// </summary>
    /// <param name="id">The ID of the quote to delete</param>
    /// <returns>True if deleted, false if not found</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        var quote = await GetByIdAsync(id);
        if (quote == null)
        {
            _logger.LogWarning("Attempted to delete non-existent motivation quote {Id}", id);
            return false;
        }
        
        _context.MotivationQuotes.Remove(quote);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Deleted motivation quote {Id}", id);
        return true;
    }
    
    /// <summary>
    /// Checks if a quote with the same author and text already exists
    /// </summary>
    /// <param name="author">The author name</param>
    /// <param name="quoteText">The quote text</param>
    /// <param name="excludeId">Optional ID to exclude from check (for updates)</param>
    /// <returns>True if duplicate exists</returns>
    public async Task<bool> IsDuplicateAsync(string author, string quoteText, int? excludeId = null)
    {
        var query = _context.MotivationQuotes
            .Where(q => q.Author == author && q.Quote == quoteText);
            
        if (excludeId.HasValue)
        {
            query = query.Where(q => q.Id != excludeId.Value);
        }
        
        return await query.AnyAsync();
    }
}