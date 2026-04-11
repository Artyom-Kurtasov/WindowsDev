using WindowsDev.Business.DataBase;
using WindowsDev.Domain;


namespace WindowsDev.Business.Services.TaskService.Attachment
{
    public class FileWriter
    {
        private readonly SharedDataService _sharedDataService;
        private readonly DbManager _dbManager;

        public FileWriter(DbManager dbManager,
            SharedDataService sharedDataService)
        {
            _dbManager = dbManager;
            _sharedDataService = sharedDataService;
        }

        public async Task AddFileInfoToDatavase(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);

            if (filePath != null)
            {
                if (fileInfo.Exists)
                {
                    using var dbContext = _dbManager.Create();

                    await dbContext.AddAsync(new TaskAttachment
                    {
                        FileName = fileInfo.Name,
                        FilePath = filePath,
                        FileExtension = fileInfo.Extension,
                        FileSize = fileInfo.Length,
                        TaskId = _sharedDataService.CurrentTask.Id
                    });

                    await dbContext.SaveChangesAsync();
                }
            }
        }
    }
}

