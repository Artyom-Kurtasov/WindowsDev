using Microsoft.EntityFrameworkCore;
using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Businnes.DataBase
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<UserAuthInfo> UserAuthInfo { get; set; }
    }
}
