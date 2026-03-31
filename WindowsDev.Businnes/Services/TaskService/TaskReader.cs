using Microsoft.EntityFrameworkCore;
using WindowsDev.Businnes.DataBase;
using WindowsDev.Businnes.Services.TaskService.Interfaces;
using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Businnes.Services.TaskService
{
    public class TaskReader : ITaskReader
    {
        private readonly AppDbContext _appDbContext;

        public TaskReader(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<List<TasksInfo>> GetTasksAsync()
        {
            return await _appDbContext.TasksInfo.ToListAsync();
        }
    }
}
