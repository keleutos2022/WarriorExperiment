using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using WarriorExperiment.Persistence.Entities;

namespace WarriorExperiment.Core.Services;

/// <summary>
/// Service for resetting user passwords during debugging
/// </summary>
public class WaPasswordResetService
{
    private readonly UserManager<WaUser> _userManager;
    private readonly ILogger<WaPasswordResetService> _logger;

    public WaPasswordResetService(
        UserManager<WaUser> userManager,
        ILogger<WaPasswordResetService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Resets password for a specific user
    /// </summary>
    public async Task<bool> ResetUserPasswordAsync(string userName, string newPassword)
    {
        try
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                _logger.LogWarning("User {UserName} not found", userName);
                return false;
            }

            _logger.LogInformation("Resetting password for user {UserName}", userName);

            // Remove existing password
            var removeResult = await _userManager.RemovePasswordAsync(user);
            if (!removeResult.Succeeded)
            {
                _logger.LogError("Failed to remove password for {UserName}: {Errors}", 
                    userName, string.Join(", ", removeResult.Errors.Select(e => e.Description)));
                return false;
            }

            // Add new password
            var addResult = await _userManager.AddPasswordAsync(user, newPassword);
            if (!addResult.Succeeded)
            {
                _logger.LogError("Failed to add password for {UserName}: {Errors}", 
                    userName, string.Join(", ", addResult.Errors.Select(e => e.Description)));
                return false;
            }

            _logger.LogInformation("Successfully reset password for user {UserName}", userName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while resetting password for {UserName}", userName);
            return false;
        }
    }

    /// <summary>
    /// Tests password verification for a user
    /// </summary>
    public async Task<bool> TestPasswordAsync(string userName, string password)
    {
        try
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                _logger.LogWarning("User {UserName} not found for password test", userName);
                return false;
            }

            var result = await _userManager.CheckPasswordAsync(user, password);
            _logger.LogInformation("Password test for {UserName} with password '{Password}': {Result}", 
                userName, password, result ? "SUCCESS" : "FAILED");
                
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during password test for {UserName}", userName);
            return false;
        }
    }
}