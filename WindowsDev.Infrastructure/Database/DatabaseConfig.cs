using WindowsDev.Application.DatabaseInterfaces;

namespace WindowsDev.Infrastructure.Database;

internal class DatabaseConfig : IDatabaseConfig
{
    public string ConnectionString { get; set; }
}