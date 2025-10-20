using System.ComponentModel.DataAnnotations;

namespace WarriorExperiment.App.Components.Pages.Account;

public class LoginModel
{
    [Required(ErrorMessage = "Username is required")]
    public string Email { get; set; } = string.Empty; // Keep property name for compatibility
    
    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
    
    public bool RememberMe { get; set; }
}