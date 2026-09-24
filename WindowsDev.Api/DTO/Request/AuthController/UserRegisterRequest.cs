using System.ComponentModel.DataAnnotations;

namespace WindowsDev.Api.DTO.Request.AuthController;

public class UserRegisterRequest
{
    [Required]
    public string Password { get; set; } = string.Empty;
    [Required]
    public string Login { get; set; } = string.Empty;
    [Required]
    public string Username { get; set; } = string.Empty;
}
