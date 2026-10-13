using System.ComponentModel.DataAnnotations;

namespace WindowsDev.Contracts.Api.Projects;

public class UpdateProjectRequest
{
    [Range(1, int.MaxValue)]
    public int Id { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    public string? Description { get; set; } = string.Empty;
}