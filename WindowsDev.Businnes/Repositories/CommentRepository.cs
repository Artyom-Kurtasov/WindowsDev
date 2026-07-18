using Microsoft.EntityFrameworkCore;
using WindowsDev.Business.DataBase.Interfaces;
using WindowsDev.Business.Repositories.Interfaces;
using WindowsDev.Domain.TasksModels;

namespace WindowsDev.Business.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly IDbManager _dbManager;

        public CommentRepository(IDbManager dbManager)
        {
            _dbManager = dbManager;
        }

        public async Task AddComments(Comments comment)
        {
            using var dbContext = _dbManager.Create();

            await dbContext.Comments.AddAsync(comment);
            await dbContext.SaveChangesAsync();
        }

        public async Task<List<Comments>> GetComments(int taskId)
        {
            using var dbContext = _dbManager.Create();

            return await dbContext.Comments.Where(x => x.TaskId == taskId).ToListAsync();
        }
    }
}
