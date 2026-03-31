namespace WindowsDev.Businnes.Services.UserManager
{
    public class CurrentUserService
    {
        private readonly CurrentUserData _currentUserData;

        public CurrentUserService(CurrentUserData currentUserData)
        {
            _currentUserData = currentUserData;
        }

        public void setUser(int id, string login, string username)
        {
            _currentUserData.Login = login;
            _currentUserData.UserId = id;
            _currentUserData.Username = username;
        }
    }
}
