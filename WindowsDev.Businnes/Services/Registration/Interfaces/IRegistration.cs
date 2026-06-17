namespace WindowsDev.Business.Services.Registration.Interfaces
{
    public interface IRegistration
    {
        Task<int> Register(string password, string login, string username);
    }
}
