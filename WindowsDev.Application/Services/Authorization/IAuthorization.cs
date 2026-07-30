using WindowsDev.Application.Primitives;

namespace WindowsDev.Application.Services.Authorization
{
    public interface IAuthorization
    {
        Task<Result<bool>> Authorize(string login, string password);
    }
}
