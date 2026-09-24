using System.ComponentModel.DataAnnotations;

namespace WindowsDev.Api.DTO.Request.ProjectsController;

public class DeleteRequest
{
    [Range(1, int.MaxValue)]
    public int Id { get; set; }
}