using System.ComponentModel.DataAnnotations;

namespace CTD_FINAL.Models.Account;

public class ForgotPasswordViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}
