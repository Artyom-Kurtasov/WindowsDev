using System.ComponentModel.DataAnnotations;

namespace WindowsDev.Contracts.Api.Profiles;

public class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;
    [Required]
    public string NewPassword { get; set; } = string.Empty;
    [Required]
    public string ConfirmPassword { get; set; } = string.Empty;
    [Required]
    public string Login { get; set; } = string.Empty;
}