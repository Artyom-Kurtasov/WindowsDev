using WindowsDev.Domain.Entities;

namespace WindowsDev.Application.Identity;

public interface IJwtService
{
    string GenerateToken(int id, string login, string username);
    UserRefreshToken GenerateRefreshToken(int userId);
}
