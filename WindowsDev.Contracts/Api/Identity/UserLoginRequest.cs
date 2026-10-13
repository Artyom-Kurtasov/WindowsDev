using System.ComponentModel.DataAnnotations;

namespace WindowsDev.Contracts.Api.Identity;

public class UserLoginRequest
{
    [Required]
    public string Login { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
}
