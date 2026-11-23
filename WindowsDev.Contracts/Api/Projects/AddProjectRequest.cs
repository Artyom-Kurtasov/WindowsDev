using System.ComponentModel.DataAnnotations;

namespace WindowsDev.Contracts.Api.Projects;

public class AddProjectRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

}