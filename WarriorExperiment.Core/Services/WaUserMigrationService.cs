using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WarriorExperiment.Persistence.Data;
using WarriorExperiment.Persistence.Entities;

namespace WarriorExperiment.Core.Services;

/// <summary>
/// Service for migrating existing users to work with ASP.NET Core Identity
/// </summary>
public class WaUserMigrationService
{
    private readonly WaDbContext _context;
    private readonly UserManager<WaUser> _userManager;
    private readonly ILogger<WaUserMigrationService> _logger;
    private const string DefaultPassword = "123";

    /// <summary>
    /// Initializes a new instance of the WaUserMigrationService
    /// </summary>
    public WaUserMigrationService(
        WaDbContext context,
        UserManager<WaUser> userManager,
        ILogger<WaUserMigrationService> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Migrates all users that need Identity setup
    /// </summary>
    /// <returns>Number of users migrated</returns>
    public async Task<int> MigrateUsersAsync()
    {
        _logger.LogInformation("Starting user migration to Identity system...");
        
        try
        {
            // Find users that need migration (missing password hash)
            var usersNeedingMigration = await _context.Users
                .Where(u => string.IsNullOrEmpty(u.PasswordHash))
                .ToListAsync();

            if (!usersNeedingMigration.Any())
            {
                _logger.LogInformation("No users require migration.");
                return 0;
            }

            _logger.LogInformation("Found {Count} users requiring migration", usersNeedingMigration.Count);

            int migratedCount = 0;
            foreach (var user in usersNeedingMigration)
            {
                try
                {
                    if (await MigrateUserAsync(user))
                    {
                        migratedCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to migrate user {UserId}: {UserName}", user.Id, user.DisplayName);
                }
            }

            _logger.LogInformation("Successfully migrated {MigratedCount} of {TotalCount} users", 
                migratedCount, usersNeedingMigration.Count);

            return migratedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user migration process");
            throw;
        }
    }

    /// <summary>
    /// Migrates a single user to Identity system
    /// </summary>
    /// <param name="user">The user to migrate</param>
    /// <returns>True if migration was successful</returns>
    private async Task<bool> MigrateUserAsync(WaUser user)
    {
        _logger.LogDebug("Migrating user {UserId}: {DisplayName}", user.Id, user.DisplayName);

        // Set up username if missing
        if (string.IsNullOrEmpty(user.UserName))
        {
            user.UserName = $"user{user.Id}";
        }

        // Preserve original username as display name if not already set
        if (string.IsNullOrEmpty(user.DisplayName))
        {
            user.DisplayName = user.UserName;
        }

        // Ensure DisplayName is set
        if (string.IsNullOrEmpty(user.DisplayName))
        {
            user.DisplayName = $"User {user.Id}";
        }

        // Set up normalized fields
        user.NormalizedUserName = user.UserName.ToUpperInvariant();
        user.NormalizedEmail = null; // No email required

        // Update timestamps
        user.UpdatedAt = DateTime.UtcNow;

        // Set password if not already set
        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            var passwordResult = await _userManager.AddPasswordAsync(user, DefaultPassword);
            if (!passwordResult.Succeeded)
            {
                _logger.LogWarning("Failed to set password for user {UserId}: {Errors}", 
                    user.Id, string.Join(", ", passwordResult.Errors.Select(e => e.Description)));
                return false;
            }
        }

        // Update the user in database
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            _logger.LogWarning("Failed to update user {UserId}: {Errors}", 
                user.Id, string.Join(", ", updateResult.Errors.Select(e => e.Description)));
            return false;
        }

        _logger.LogDebug("Successfully migrated user {UserId}: {Email}", user.Id, user.Email);
        return true;
    }

    /// <summary>
    /// Gets count of users that need migration
    /// </summary>
    /// <returns>Number of users needing migration</returns>
    public async Task<int> GetUsersNeedingMigrationCountAsync()
    {
        return await _context.Users
            .CountAsync(u => string.IsNullOrEmpty(u.PasswordHash));
    }

    /// <summary>
    /// Checks if any users need migration
    /// </summary>
    /// <returns>True if users need migration</returns>
    public async Task<bool> HasUsersNeedingMigrationAsync()
    {
        return await _context.Users
            .AnyAsync(u => string.IsNullOrEmpty(u.PasswordHash));
    }
}