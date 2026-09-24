using System.Diagnostics;
using WindowsDev.Application.Tasks.Attachment;

namespace WindowsDev.Infrastructure.FileOpener;

internal class FileOpener : IFileOpener
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