using WindowsDev.Application.DatabaseInterfaces;

namespace WindowsDev.Infrastructure.Database
{
    public class DatabaseConfig : IDatabaseConfig
    {
        public string ConnectionString { get; set; }
    }
}
