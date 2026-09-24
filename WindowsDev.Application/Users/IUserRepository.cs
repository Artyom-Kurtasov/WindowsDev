using WindowsDev.Domain.Entities;

namespace WindowsDev.Application.Users;

public interface IUserRepository
{
    Task<bool> ExistsByLoginAsync(string login);
    Task<UserInfo?> GetByLoginAsync(string login);
    Task<bool> ExistsByUsernameAsync(string username);
    Task AddAsync(UserInfo user);
    Task UpdateAsync(UserInfo user);
}