using System.Collections.ObjectModel;
using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Businnes.Services.ProjectService.Interfaces
{
    public interface IProjectLoader
    {
        Task<ObservableCollection<ProjectsInfo>> LoadProjectAsync();
    }
}
