using System.ComponentModel.DataAnnotations;

namespace WarriorExperiment.App.Components.Pages.Account;

public class RegisterModel
{
    [Required(ErrorMessage = "Display name is required")]
    [StringLength(100, ErrorMessage = "Display name cannot exceed 100 characters")]
    public string DisplayName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Username is required")]
    public string Email { get; set; } = string.Empty; // Keep property name for compatibility
    
    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Password confirmation is required")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}