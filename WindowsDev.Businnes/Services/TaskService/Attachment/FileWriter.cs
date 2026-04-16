using System.Windows.Documents;
using WindowsDev.Business.DataBase;
using WindowsDev.Domain.TasksModels;


namespace WindowsDev.Business.Services.TaskService.Attachment
{
    public class FileWriter
    {
        private readonly DbManager _dbManager;

        public FileWriter(DbManager dbManager)
        {
            _dbManager = dbManager;
        }

        public async Task<TaskAttachment?> AddFileInfoToDatabase(string filePath, int taskId)
        {
            FileInfo fileInfo = new FileInfo(filePath);

            if (fileInfo.Exists)
            {
                using var dbContext = _dbManager.Create();

                TaskAttachment attachment = new TaskAttachment
                {
                    FileName = fileInfo.Name,
                    FilePath = filePath,
                    FileExtension = fileInfo.Extension,
                    FileSize = fileInfo.Length,
                    TaskId = taskId
                };

                await dbContext.AddAsync(attachment);
                await dbContext.SaveChangesAsync();

                return attachment;
            }

            return null;
        }
    }
}


