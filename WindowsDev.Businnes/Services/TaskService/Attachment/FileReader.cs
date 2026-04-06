using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using WindowsDev.Businnes.DataBase;
using WindowsDev.Domain;
using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Businnes.Services.TaskService.Attachment
{
    public class FileReader
    {
        private readonly AppDbContext _appDbContext;

        public FileReader(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<ObservableCollection<TaskAttachment>> GetAttachmentsAsync(TasksInfo taskInfo)
        {
            var attacments = await _appDbContext.Attachments
                 .Where(x => x.TaskId == taskInfo.Id)
                 .ToListAsync();

            return new ObservableCollection<TaskAttachment>(attacments);
        }
    }
}
