using WindowsDev.Application.Database;

namespace WindowsDev.Infrastructure.Database;

internal class DatabaseConfig : IDatabaseConfig
{
    public string ConnectionString { get; set; }
}