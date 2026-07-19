using System.ComponentModel.DataAnnotations;

namespace CTD_FINAL.Models.Account;

public class LoginViewModel
{
    [Required(ErrorMessage = "License number is required")]
    [Display(Name = "License Number")]
    public string LicenseNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Username or email is required")]
    [Display(Name = "Username or Email")]
    public string UserNameOrEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}
