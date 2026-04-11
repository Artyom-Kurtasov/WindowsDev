using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using WindowsDev.Business.DataBase;
using WindowsDev.Domain;
using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Business.Services.TaskService.Attachment
{
    public class FileReader
    {
        private readonly DbManager _dbManager;

        public FileReader(DbManager dbManager)
        {
            _dbManager = dbManager;
        }

        public async Task<ObservableCollection<TaskAttachment>> GetAttachmentsAsync(TasksInfo taskInfo)
        {
            using var dbContext = _dbManager.Create();

            var attacments = await dbContext.Attachments
                 .Where(x => x.TaskId == taskInfo.Id)
                 .ToListAsync();

            return new ObservableCollection<TaskAttachment>(attacments);
        }
    }
}


