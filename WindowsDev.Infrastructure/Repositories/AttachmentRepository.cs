using Microsoft.EntityFrameworkCore;
using WindowsDev.Application.Tasks.Attachment;
using WindowsDev.Domain.Entities;
using WindowsDev.Infrastructure.Database.Interfaces;

namespace WindowsDev.Infrastructure.Repositories;

internal class AttachmentRepository : IAttachmentRepository
{
    private readonly IDbCreator _dbManager;

    public AttachmentRepository(IDbCreator dbManager)
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

        var attachments = await dbContext
            .Attachments.Where(x => x.TaskId == taskId)
            .ToListAsync();

        return attachments;
    }

    public async Task<TaskAttachment?> GetAttachmentAsync(int attachmentId)
    {
        using var dbContext = _dbManager.Create();

        return await dbContext.Attachments.FirstOrDefaultAsync(x => x.Id == attachmentId);
    }
}