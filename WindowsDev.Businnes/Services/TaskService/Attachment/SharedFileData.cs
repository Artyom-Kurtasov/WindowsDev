using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WindowsDev.Businnes.Services.TaskService.Attachment
{
    public class SharedFileData : INotifyPropertyChanged
    {
        private string? _fileType;
        public string? FileType
        {
            get => _fileType;
            set
            {
                _fileType = value;
                OnPropertyChanged();
            }
        }

        private string? _filePath;
        public string? FilePath
        {
            get => _filePath; set
            {
                _filePath = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string? property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
