using System.ComponentModel.DataAnnotations;

namespace WindowsDev.Api.DTO.Request;

public class ChangePasswordRequest
{
    [Required]
    public string Login { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
}