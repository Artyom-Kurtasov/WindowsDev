using Microsoft.EntityFrameworkCore;
using WindowsDev.Domain;
using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Business.DataBase
{
    public class AppDbContext : DbContext
    {
        private string _dbConnectionString;
        public AppDbContext(string dbConnectionString)
        {
            _dbConnectionString = dbConnectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_dbConnectionString);
        }
        public DbSet<UsersInfo> UsersInfo { get; set; }
        public DbSet<ProjectsInfo> ProjectsInfo { get; set; }
        public DbSet<TasksInfo> TasksInfo { get; set; }
        public DbSet<Comments> Comments { get; set; }
        public DbSet<TaskAttachment> Attachments { get; set; }
    }
}

