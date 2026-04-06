using System.Collections.ObjectModel;
using WindowsDev.Businnes.Services.ProjectService.Interfaces;
using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Businnes.Services.ProjectService
{
    /// <summary>
    /// Loads projects via IProjectReader and wraps them in an ObservableCollection.
    /// </summary>
    public class ProjectLoader : IProjectLoader
    {
        private readonly IProjectReader _projectReader;

        public ProjectLoader(IProjectReader projectReader)
        {
            _projectReader = projectReader;
        }

        /// <summary>
        /// Loads all projects for the current user asynchronously.
        /// </summary>
        public async Task<ObservableCollection<ProjectsInfo>> LoadProjectAsync()
        {
            var projectsList = await _projectReader.GetProjectsAsync();
            return new ObservableCollection<ProjectsInfo>(projectsList);
        }
    }
}