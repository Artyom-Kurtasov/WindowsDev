namespace WindowsDev.Api.DTO.Response.ProjectsController;

public class AddResponse
{
    public int Id { get; set; }
    public string? Description { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}