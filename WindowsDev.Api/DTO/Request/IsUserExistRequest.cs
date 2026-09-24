using System.ComponentModel.DataAnnotations;

namespace WindowsDev.Api.DTO.Request;

public class IsUserExistRequest
{
    [Required]
    public string Login { get; set; } = string.Empty;
}