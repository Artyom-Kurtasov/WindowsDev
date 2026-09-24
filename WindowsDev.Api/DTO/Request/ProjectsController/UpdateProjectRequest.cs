using System.ComponentModel.DataAnnotations;

namespace WindowsDev.Api.DTO.Request.ProjectsController;

public class UpdateProjectRequest
{
    [Range(1, int.MaxValue)]
    public int Id { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    public string? Description { get; set; } = string.Empty;
}