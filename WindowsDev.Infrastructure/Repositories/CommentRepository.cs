using Microsoft.EntityFrameworkCore;
using WindowsDev.Application.RepositoriesInterfaces;
using WindowsDev.Domain.Entities;
using WindowsDev.Infrastructure.Database.Interfaces;

namespace WindowsDev.Infrastructure.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly IDbCreator _dbManager;

        public CommentRepository(IDbCreator dbManager)
        {
            _dbManager = dbManager;
        }

        public async Task AddComments(TaskComment comment)
        {
            using var dbContext = _dbManager.Create();

            await dbContext.Comments.AddAsync(comment);
            await dbContext.SaveChangesAsync();
        }

        public async Task<List<TaskComment>> GetComments(int taskId)
        {
            using var dbContext = _dbManager.Create();

            return await dbContext.Comments.Where(x => x.TaskId == taskId).ToListAsync();
        }
    }
}
