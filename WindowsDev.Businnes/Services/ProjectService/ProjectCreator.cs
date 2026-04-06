using WindowsDev.Businnes.Services.ProjectService.Interfaces;
using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Businnes.Services.ProjectService
{
    /// <summary>
    /// Responsible for creating new projects and saving them via IProjectWriter.
    /// </summary>
    public class ProjectCreator : IProjectCreator
    {
        private readonly IProjectWriter _projectWriter;

        public ProjectCreator(IProjectWriter projectWriter)
        {
            _projectWriter = projectWriter;
        }

        /// <summary>
        /// Creates a new project and saves it to the database.
        /// </summary>
        public async Task CreateProject(string name, string description, int userId)
        {
            var project = new ProjectsInfo
            {
                Name = name,
                Description = description,
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            };

            await _projectWriter.AddAsync(project);
        }
    }
}