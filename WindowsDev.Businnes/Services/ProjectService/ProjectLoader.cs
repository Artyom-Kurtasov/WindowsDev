using System.Collections.ObjectModel;
using WindowsDev.Businnes.Services.ProjectService.Interfaces;
using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Businnes.Services.ProjectService
{
    public class ProjectLoader
    {
        private readonly IProjectReader _projectReader;

        public ProjectLoader(IProjectReader projectReader)
        {
            _projectReader = projectReader;
        }
        public async Task<ObservableCollection<Project>> LoadProjectAsync()
        {
            var projectsList = await _projectReader.GetProjectsAsync();
            return new ObservableCollection<Project>(projectsList);
        }
    }
}
