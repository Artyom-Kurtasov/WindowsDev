using System.ComponentModel.DataAnnotations;

namespace WindowsDev.Contracts.Api.Projects;

public class AddProjectRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

}