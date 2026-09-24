using System.ComponentModel.DataAnnotations;

namespace WindowsDev.Api.DTO.Response;

public class ChangePasswordResponse
{
    [Required]
    public int RecoveryCode { get; set; }
}