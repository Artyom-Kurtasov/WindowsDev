using System.ComponentModel.DataAnnotations;

namespace WindowsDev.Contracts.Api.PasswordRecovery;

public class IsUserExistRequest
{
    [Required]
    public string Login { get; set; } = string.Empty;
}