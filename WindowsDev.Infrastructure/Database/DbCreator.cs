using Microsoft.EntityFrameworkCore;
using WindowsDev.Application.DatabaseInterfaces;
using WindowsDev.Infrastructure.Database.Interfaces;

namespace WindowsDev.Infrastructure.Database
{
    public class DbCreator : IDbCreator
    {
        private readonly IDatabaseConfig _config;

        public DbCreator(IDatabaseConfig config)
        {
            _config = config;
        }

        public AppDbContext Create()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(_config.ConnectionString)
                .Options;

            return new AppDbContext(options);
        }
    }
}
