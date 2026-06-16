using Microsoft.EntityFrameworkCore;
using WindowsDev.Domain.ProjectsModels;
using WindowsDev.Domain.TasksModels;
using WindowsDev.Domain.UsersModels;

namespace WindowsDev.Business.DataBase
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // Enums are stored as strings in the database instead of integers.
            // This makes the database human-readable and protects against
            // enum reordering breaking existing data
            modelBuilder.Entity<TasksInfo>()
                .Property(x => x.Status)
                .HasConversion<string>();

            modelBuilder.Entity<TasksInfo>()
                .Property(x => x.Priority)
                .HasConversion<string>();

            modelBuilder.Entity<TasksInfo>()
                .Property(x => x.Progress)
                .HasConversion<string>();
            modelBuilder.Entity<UsersInfo>()
                .Property(x => x.HashMethod)
                .HasConversion<string>();
        }
        public virtual DbSet<UsersInfo> UsersInfo { get; set; }
        public virtual DbSet<ProjectsInfo> ProjectsInfo { get; set; }
        public virtual DbSet<TasksInfo> TasksInfo { get; set; }
        public virtual DbSet<Comments> Comments { get; set; }
        public virtual DbSet<TaskAttachment> Attachments { get; set; }
    }
}


