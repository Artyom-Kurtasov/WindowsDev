using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Businnes.Services
{
    public class SharedDataService : INotifyPropertyChanged
    {
        private ObservableCollection<ProjectsInfo>? _projectList;
        public ObservableCollection<ProjectsInfo>? ProjectList
        {
            get => _projectList;
            set
            {
                _projectList = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<TasksInfo>? _taskList;
        public ObservableCollection<TasksInfo>? TaskList
        {
            get => _taskList;
            set
            {
                _taskList = value;
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
