using System.ComponentModel;
using WindowsDev.Business.Services.UserManager.Interfaces;

namespace WindowsDev.Business.Services.UserManager
{
    public class CurrentUserService : INotifyPropertyChanged, ICurrentUserService
    {

        private string _login = string.Empty;
        public string Login
        {
            get => _login;
            set
            {
                _login = value;
                OnPropertyChanged(nameof(Login));
            }
        }

        private string _username = string.Empty;
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        private int _userId;
        public int UserId
        {
            get => _userId;
            set
            {
                _userId = value;
                OnPropertyChanged(nameof(UserId));
            }
        }

        public void SetUser(int id, string login, string username)
        {
            Login = login;
            UserId = id;
            Username = username;
        }

        public void ClearUser()
        {
            Login = string.Empty;
            UserId = -1;
            Username = string.Empty;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}


