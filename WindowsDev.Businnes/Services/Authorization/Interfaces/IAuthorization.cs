using WindowsDev.Business.Primitives;

namespace WindowsDev.Business.Services.Authorization.Interfaces
{
    public interface IAuthorization
    {
        Task<Result<bool>> Authorize(string login, string password);
    }
}
