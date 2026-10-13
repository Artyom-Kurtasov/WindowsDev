using System.ComponentModel.DataAnnotations;

namespace WindowsDev.Contracts.Api.PasswordRecovery;

public class ResetPasswordRequest
{
    [Required]
    public string Login { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
}