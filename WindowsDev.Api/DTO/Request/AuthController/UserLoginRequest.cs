using System.ComponentModel.DataAnnotations;

namespace WindowsDev.Api.DTO.Request.AuthController;

public class UserLoginRequest
{
    [Required]
    public string Login { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
}
