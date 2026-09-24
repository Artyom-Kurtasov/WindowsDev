using System.ComponentModel.DataAnnotations;

namespace WindowsDev.Api.DTO.Request.ProjectService;

public class AddProjectRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

}