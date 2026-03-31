using WindowsDev.Businnes.DataBase;
using WindowsDev.Businnes.Services.TaskService.Interfaces;
using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Businnes.Services.TaskService
{
    public class TaskWriter : ITaskWriter
    {
        private readonly AppDbContext _appDbContext;

        public TaskWriter(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task AddAsync(TasksInfo task)
        {
            await _appDbContext.AddAsync(task);
            await _appDbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var task = await _appDbContext.TasksInfo.FindAsync(id);

            if (task != null)
            {
                _appDbContext.TasksInfo.Remove(task);
                await _appDbContext.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(TasksInfo task)
        {
            _appDbContext.TasksInfo.Update(task);
            await _appDbContext.SaveChangesAsync();
        }
    }
}
