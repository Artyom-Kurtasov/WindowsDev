namespace WindowsDev.Contracts.UI.Projects;

public record ProjectInfo(
    int Id,
    string Name,
    string? Description,
    DateTime CreatedAt);