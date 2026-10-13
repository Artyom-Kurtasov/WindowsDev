using System.ComponentModel.DataAnnotations;

namespace WindowsDev.Contracts.Api.PasswordRecovery;

public class ResetPasswordResponse
{
    [Required]
    public int RecoveryCode { get; set; }
}