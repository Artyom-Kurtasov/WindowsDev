using System.ComponentModel.DataAnnotations;

namespace WindowsDev.Api.DTO.Response.AuthController;

public class UserLoginResponse
{
    [Required]
    public string JwtToken { get; set; } = string.Empty;
}
