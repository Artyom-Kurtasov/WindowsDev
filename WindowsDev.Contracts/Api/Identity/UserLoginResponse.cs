using System.ComponentModel.DataAnnotations;

namespace WindowsDev.Contracts.Api.Identity;

public class UserLoginResponse
{
    [Required]
    public string JwtToken { get; set; } = string.Empty;
}
