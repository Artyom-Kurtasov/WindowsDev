namespace WindowsDev.Business.Services.UserManager.Interfaces
{
    public interface ICurrentUserService
    {
        public string Login { get; set; }
        public string Username { get; set; }
        public int UserId { get; set; }
        void SetUser(int id, string login, string username);
        void ClearUser();
    }
}
