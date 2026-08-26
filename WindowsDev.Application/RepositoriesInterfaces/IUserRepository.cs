using WindowsDev.Domain.Entities;

namespace WindowsDev.Application.RepositoriesInterfaces;

public interface IUserRepository
{
    Task<bool> ExistsByLoginAsync(string login);
    Task<User?> GetByLoginAsync(string login);
    Task<bool> ExistsByUsernameAsync(string username);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
}