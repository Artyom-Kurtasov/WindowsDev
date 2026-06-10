namespace WindowsDev.Business.Services.Authorization.Interfaces
{
    public interface IAuthorization
    {
        Task<bool> Authorize(string login, string password);
    }
}
