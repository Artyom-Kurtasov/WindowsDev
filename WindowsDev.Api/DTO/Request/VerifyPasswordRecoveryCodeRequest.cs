using System.ComponentModel.DataAnnotations;

namespace WindowsDev.Api.DTO.Request;

public class VerifyPasswordRecoveryCodeRequest
{
    [Required]
    public int recoveryCode { get; set; }

    [Required]
    public string login { get; set; } = string.Empty;
}