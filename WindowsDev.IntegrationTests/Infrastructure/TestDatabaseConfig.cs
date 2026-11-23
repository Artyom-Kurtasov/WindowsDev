using WindowsDev.Application.Database;

namespace WindowsDev.IntegrationTests.Infrastructure;

internal sealed class TestDatabaseConfig : IDatabaseConfig
{
    public TestDatabaseConfig(string connectionString)
    {
        ConnectionString = connectionString;
    }

    public string ConnectionString { get; set; }
}
