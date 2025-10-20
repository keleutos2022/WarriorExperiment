using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using WarriorExperiment.Persistence.Entities;

namespace WarriorExperiment.App.Controllers;

/// <summary>
/// Controller for handling authentication operations that require HTTP header modifications
/// </summary>
[Route("[controller]")]
public class AccountController : Controller
{
    private readonly SignInManager<WaUser> _signInManager;
    private readonly UserManager<WaUser> _userManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        SignInManager<WaUser> signInManager,
        UserManager<WaUser> userManager,
        ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Handles login form submission
    /// </summary>
    [HttpPost("ProcessLogin")]
    public async Task<IActionResult> ProcessLogin(LoginModel model, string? returnUrl = null)
    {
        try
        {
            _logger.LogInformation("=== LOGIN ATTEMPT DEBUG (Controller) ===");
            _logger.LogInformation("Username: '{Username}', Password length: {PasswordLength}", model.Username, model.Password?.Length);

            // Validate model
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Login model validation failed");
                return Redirect($"/Account/Login?error=validation&returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}");
            }

            // Check if user exists
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
            {
                _logger.LogWarning("User '{Username}' not found in database", model.Username);
                return Redirect($"/Account/Login?error=invalid&returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}");
            }

            _logger.LogInformation("User found: ID={UserId}, UserName='{UserName}', DisplayName='{DisplayName}'", 
                user.Id, user.UserName, user.DisplayName);
            _logger.LogInformation("User has password hash: {HasPassword}", !string.IsNullOrEmpty(user.PasswordHash));

            // Check password directly
            var passwordCheck = await _userManager.CheckPasswordAsync(user, model.Password);
            _logger.LogInformation("Password check result: {PasswordCheck}", passwordCheck);

            // Try SignIn - this should work in controller context
            var result = await _signInManager.PasswordSignInAsync(
                model.Username,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            _logger.LogInformation("SignIn result - Succeeded: {Succeeded}, IsLockedOut: {IsLockedOut}, RequiresTwoFactor: {RequiresTwoFactor}, IsNotAllowed: {IsNotAllowed}",
                result.Succeeded, result.IsLockedOut, result.RequiresTwoFactor, result.IsNotAllowed);

            if (result.Succeeded)
            {
                _logger.LogInformation("Login successful for user '{Username}'", model.Username);
                var redirectUrl = !string.IsNullOrEmpty(returnUrl) ? Uri.UnescapeDataString(returnUrl) : "/";
                return LocalRedirect(redirectUrl);
            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning("Account locked for user '{Username}'", model.Username);
                return Redirect($"/Account/Login?error=locked&returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}");
            }
            else
            {
                var reason = result.IsNotAllowed ? "not-allowed" :
                            result.RequiresTwoFactor ? "two-factor" : "invalid";
                _logger.LogWarning("Login failed for user '{Username}': {Reason}", model.Username, reason);
                return Redirect($"/Account/Login?error={reason}&returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during login for user '{Username}'", model.Username);
            return Redirect($"/Account/Login?error=exception&returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}");
        }
        finally
        {
            _logger.LogInformation("=== END LOGIN DEBUG ===");
        }
    }

    /// <summary>
    /// Handles registration form submission
    /// </summary>
    [HttpPost("ProcessRegister")]
    public async Task<IActionResult> ProcessRegister(RegisterModel model)
    {
        try
        {
            _logger.LogInformation("=== REGISTRATION ATTEMPT DEBUG (Controller) ===");
            _logger.LogInformation("DisplayName: '{DisplayName}', Username: '{Username}', Password length: {PasswordLength}",
                model.DisplayName, model.Username, model.Password?.Length);

            // Validate model
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Registration model validation failed");
                return Redirect("/Account/Register?error=validation");
            }

            // Check if username already exists
            var existingUser = await _userManager.FindByNameAsync(model.Username);
            if (existingUser != null)
            {
                _logger.LogWarning("Username '{Username}' already exists", model.Username);
                return Redirect("/Account/Register?error=username-exists");
            }

            var user = new WaUser
            {
                UserName = model.Username,
                Email = null, // No email required
                DisplayName = model.DisplayName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                EnteredAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created successfully: ID={UserId}", user.Id);

                // Sign in the user immediately
                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation("User signed in successfully");

                return LocalRedirect("/");
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));

                // Log detailed error information for debugging
                foreach (var error in result.Errors)
                {
                    _logger.LogError("Identity Error: {Code} - {Description}", error.Code, error.Description);
                }

                return Redirect($"/Account/Register?error=identity&details={Uri.EscapeDataString(errors)}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during registration for user '{Username}'", model.Username);
            return Redirect("/Account/Register?error=exception");
        }
        finally
        {
            _logger.LogInformation("=== END REGISTRATION DEBUG ===");
        }
    }

    /// <summary>
    /// Handles logout
    /// </summary>
    [HttpPost("ProcessLogout")]
    public async Task<IActionResult> ProcessLogout()
    {
        try
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out successfully");
            return LocalRedirect("/");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during logout");
            return LocalRedirect("/");
        }
    }
}

/// <summary>
/// Model for login form data
/// </summary>
public class LoginModel
{
    [Required(ErrorMessage = "Username is required")]
    public string Username { get; set; } = "";

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = "";

    public bool RememberMe { get; set; } = false;
}

/// <summary>
/// Model for registration form data
/// </summary>
public class RegisterModel
{
    [Required(ErrorMessage = "Display name is required")]
    [StringLength(100, ErrorMessage = "Display name cannot exceed 100 characters")]
    public string DisplayName { get; set; } = "";

    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    public string Username { get; set; } = "";

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Password must be at least 1 character")]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Password confirmation is required")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = "";
}