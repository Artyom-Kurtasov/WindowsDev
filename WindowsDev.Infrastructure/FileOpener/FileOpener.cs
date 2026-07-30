using System.Diagnostics;
using WindowsDev.Application.Services.TaskService.Attachment.FileServiceInterfaces;

namespace WindowsDev.Infrastructure.FileOpener
{
    public class FileOpener : IFileOpener
    {
        public void Open(string filePath)
        {
            Process.Start(
                new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                }
            );
        }
    }
}
