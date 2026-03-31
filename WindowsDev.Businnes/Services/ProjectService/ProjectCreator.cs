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
        public async Task CreateProject(string name, string description, int userId)
        {
            ProjectsInfo project = new ProjectsInfo
            {
                Name = name,
                Description = description,
                CreatedAt = DateTime.Now.ToUniversalTime(),
                UserId = userId
            };
            await _projectWriter.AddAsync(project);
        }
    }
}
