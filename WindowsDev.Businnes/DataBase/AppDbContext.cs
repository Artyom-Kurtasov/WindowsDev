using Microsoft.EntityFrameworkCore;
using WindowsDev.Domain.UsersLoginAndPasswords;

namespace WindowsDev.Businnes.DataBase
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<UsersLoginAndPasswords> usersLoginAndPasswords { get; set; }
    }
}
