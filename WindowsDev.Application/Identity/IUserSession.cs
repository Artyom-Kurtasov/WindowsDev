namespace WindowsDev.Application.Identity
{
    public interface IUserSession
    {
        public int UserId { get; }
        public string Username { get; }
        public string Login { get; }
    }
}
