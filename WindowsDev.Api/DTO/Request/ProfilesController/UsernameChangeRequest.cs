using System.ComponentModel.DataAnnotations;

namespace WindowsDev.Api.DTO.Request.ProfilesController;

public class UsernameChangeRequest
{
    [Required]
    public string CurrentUsername { get; set; } = string.Empty;
    [Required]
    public string NewUsername { get; set; } = string.Empty;
}