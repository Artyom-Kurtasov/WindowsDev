using System.ComponentModel.DataAnnotations;

namespace WindowsDev.Api.DTO.Request.ProjectsController;

public class GetProjectsRequest
{
    public string? SearchFilter { get; set; } = null;
    [Range(1, int.MaxValue)]
    public int Page { get; set; }
    [Range(1, int.MaxValue)]
    public int Size { get; set; }
}