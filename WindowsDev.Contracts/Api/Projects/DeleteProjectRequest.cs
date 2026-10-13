using System.ComponentModel.DataAnnotations;

namespace WindowsDev.Contracts.Api.Projects;

public class DeleteProjectRequest
{
    [Range(1, int.MaxValue)]
    public int Id { get; set; }
}