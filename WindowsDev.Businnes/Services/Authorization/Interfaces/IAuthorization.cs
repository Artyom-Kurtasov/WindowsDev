namespace WindowsDev.Business.Services.Authorization.Interfaces
{
    public interface IAuthorization
    {
        Task Authorize(string login, string password);
    }
}
