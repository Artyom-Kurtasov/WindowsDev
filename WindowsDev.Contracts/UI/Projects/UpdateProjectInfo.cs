namespace WindowsDev.Contracts.UI.Projects;

public record UpdateProjectInfo(
    int Id,
    int UserId,
    string Name,
    string? Description,
    DateTime CreatedAt);
