using WindowsDev.Business.Repositories.Interfaces;
using WindowsDev.Business.Services.ProjectService.Interfaces;
using WindowsDev.Business.Services.UserManager.Interfaces;
using WindowsDev.Domain.ProjectsModels;

namespace WindowsDev.Business.Services.ProjectService
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly ICurrentUserService _currentUserService;

        public ProjectService(
            IProjectRepository projectRepository,
            ICurrentUserService currentUserService
        )
        {
            _projectRepository = projectRepository;
            _currentUserService = currentUserService;
        }

        public async Task AddAsync(ProjectsInfo project)
        {
            ArgumentNullException.ThrowIfNull(project);

            await _projectRepository.AddAsync(project);
        }

        public async Task DeleteAsync(int id)
        {
            await _projectRepository.DeleteAsync(id);
        }

        public async Task UpdateAsync(ProjectsInfo project)
        {
            ArgumentNullException.ThrowIfNull(project);

            await _projectRepository.UpdateAsync(project);
        }

        public async Task<List<ProjectsInfo>> GetProjectsAsync(
            int page,
            int size,
            string searchFilter = ""
        )
        {
            page = page < 1 ? 1 : page;
            size = size < 1 ? 1 : size;

            return await _projectRepository.GetProjectsAsync(
                page,
                size,
                _currentUserService.UserId,
                searchFilter
            );
        }

        public async Task<int> GetProjectsCountAsync()
        {
            return await _projectRepository.GetProjectsCountAsync(_currentUserService.UserId);
        }
    }
}
