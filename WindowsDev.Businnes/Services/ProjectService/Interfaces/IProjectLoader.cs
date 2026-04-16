using System.Collections.ObjectModel;
using WindowsDev.Domain.ProjectsModels;

namespace WindowsDev.Business.Services.ProjectService.Interfaces
{
    public interface IProjectLoader
    {
        Task<ObservableCollection<ProjectsInfo>> LoadProjectAsync();
    }
}


