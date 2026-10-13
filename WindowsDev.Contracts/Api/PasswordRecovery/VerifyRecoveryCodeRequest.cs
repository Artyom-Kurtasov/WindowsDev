using System.ComponentModel.DataAnnotations;

namespace WindowsDev.Contracts.Api.PasswordRecovery;

public class VerifyRecoveryCodeRequest
{
    [Required]
    public int recoveryCode { get; set; }

    [Required]
    public string login { get; set; } = string.Empty;
}