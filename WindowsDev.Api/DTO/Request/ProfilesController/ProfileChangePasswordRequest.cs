using System.ComponentModel.DataAnnotations;

namespace WindowsDev.Api.DTO.Request.ProfilesController;

public class ProfileChangePasswordRequest
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