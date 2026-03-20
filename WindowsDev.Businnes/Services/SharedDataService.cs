using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Businnes.Services
{
    public class SharedDataService : INotifyPropertyChanged
    {
        private ObservableCollection<Project>? _projectList;
        public ObservableCollection<Project>? ProjectList
        {
            get => _projectList;
            set
            {
                _projectList = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
