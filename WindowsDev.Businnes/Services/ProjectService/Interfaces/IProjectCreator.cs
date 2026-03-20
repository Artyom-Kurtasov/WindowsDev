namespace WindowsDev.Businnes.Services.ProjectService.Interfaces
{
    public interface IProjectCreator
    {
        Task CreateProject(string name, string description);
    }
}
