using WindowsDev.Domain.UsersModels;

namespace WindowsDev.Business.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> ExistsByLoginAsync(string login);
        Task<UsersInfo?> GetByLoginAsync(string login);
        Task AddAsync(UsersInfo user);
        Task UpdateAsync(UsersInfo user);
    }
}
