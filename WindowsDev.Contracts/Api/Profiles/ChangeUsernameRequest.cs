using System.ComponentModel.DataAnnotations;

namespace WindowsDev.Contracts.Api.Profiles;

public class ChangeUsernameRequest
{
    [Required]
    public string CurrentUsername { get; set; } = string.Empty;
    [Required]
    public string NewUsername { get; set; } = string.Empty;
}