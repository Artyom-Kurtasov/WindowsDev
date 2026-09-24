using Microsoft.Win32;
using WindowsDev.Application.Tasks.Attachment;

namespace WindowsDev.Services.FilePicker;

internal class FilePicker : IFilePicker
{
    public string? PickFile()
    {
        var dialog = new OpenFileDialog();
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}