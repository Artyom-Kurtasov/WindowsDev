using WindowsDev.IntegrationTests.Infrastructure;

namespace WindowsDev.IntegrationTests;

public abstract class BaseIntegrationTest
    : IClassFixture<PostgreApiFactory>, IAsyncLifetime
{
    protected readonly HttpClient Client;
    protected readonly PostgreApiFactory Factory;

    protected BaseIntegrationTest(PostgreApiFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    public Task InitializeAsync() => Factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;
}