namespace WindowsDev.Api.DTO.Response.ProjectsController;

public class GetByIdResponse
{
    public int Id { get; set; }
    public required string Name { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int UserId { get; set; }
}