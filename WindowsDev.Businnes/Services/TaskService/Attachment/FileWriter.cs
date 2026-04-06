using WindowsDev.Businnes.DataBase;
using WindowsDev.Domain;


namespace WindowsDev.Businnes.Services.TaskService.Attachment
{
    public class FileWriter
    {
        private readonly SharedDataService _sharedDataService;
        private readonly AppDbContext _appDbContext;

        public FileWriter(AppDbContext appDbContext,
            SharedDataService sharedDataService)
        {
            _appDbContext = appDbContext;
            _sharedDataService = sharedDataService;
        }

        public async Task AddFileInfoToDatavase(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);

            if (filePath != null)
            {
                if (fileInfo.Exists)
                {
                    await _appDbContext.AddAsync(new TaskAttachment
                    {
                        FileName = fileInfo.Name,
                        FilePath = filePath,
                        FileExtension = fileInfo.Extension,
                        FileSize = fileInfo.Length,
                        TaskId = _sharedDataService.CurrentTask.Id
                    });

                    await _appDbContext.SaveChangesAsync();
                }
            }
        }
    }
}
