using WindowsDev.Businnes.Services.ProjectService.Interfaces;
using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Businnes.Services.ProjectService
{
    public class ProjectCreator : IProjectCreator
    {
        private readonly IProjectWriter _projectWriter;

        public ProjectCreator(IProjectWriter projectWriter)
        {
            _projectWriter = projectWriter;
        }
        public async Task CreateProject(string name, string description)
        {
            Project project = new Project
            {
                Name = name,
                Description = description,
                Created = DateTime.Now.ToString("dd.MM.yyyy"),
            };
            await _projectWriter.AddAsync(project);
        }
    }
}
