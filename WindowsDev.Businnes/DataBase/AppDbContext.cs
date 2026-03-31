using Microsoft.EntityFrameworkCore;
using WindowsDev.Domain;
using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Businnes.DataBase
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<UsersInfo> UsersInfo { get; set; }
        public DbSet<ProjectsInfo> ProjectsInfo { get; set; }
        public DbSet<TasksInfo> TasksInfo { get; set; }
        public DbSet<Comments> Comments { get; set; }
    }
}
