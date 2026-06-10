using Microsoft.EntityFrameworkCore;
using WindowsDev.Business.DataBase.Interfaces;
using WindowsDev.Business.Repositories.Interfaces;
using WindowsDev.Domain.TasksModels;

namespace WindowsDev.Business.Repositories
{
    public class AttachmentRepository : IAttachmentRepository
    {
        private readonly IDbManager _dbManager;

        public AttachmentRepository(IDbManager dbManager)
        {
            _dbManager = dbManager;
        }
        public async Task AddFileInfoToDatabase(TaskAttachment attachment)
        {
            using var dbContext = _dbManager.Create();

            await dbContext.AddAsync(attachment);
            await dbContext.SaveChangesAsync();
        }

        public async Task<List<TaskAttachment>> GetAttachmentsAsync(int taskId)
        {
            using var dbContext = _dbManager.Create();

            var attachments = await dbContext.Attachments
                 .Where(x => x.TaskId == taskId)
                 .ToListAsync();

            return attachments;
        }
    }
}
