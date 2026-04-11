namespace WindowsDev.Business.Services.UserManager
{
    public class CurrentUserData
    {
        private string? _login;
        public string? Login
        {
            get => _login;
            set => _login = value;
        }

        private string? _username;
        public string? Username
        {
            get => _username;
            set => _username = value;
        }

        private int _userId;
        public int UserId
        {
            get => _userId;
            set => _userId = value;
        }
    }
}


