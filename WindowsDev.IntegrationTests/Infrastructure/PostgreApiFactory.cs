using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;
using WindowsDev.Application.Database;
using WindowsDev.Infrastructure.Database.Interfaces;

namespace WindowsDev.IntegrationTests.Infrastructure;

public class PostgreApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("test_db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        using var scope = Services.CreateScope();
        var dbCreator = scope.ServiceProvider.GetRequiredService<IDbCreator>();
        using var db = dbCreator.Create();
        await db.Database.EnsureCreatedAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IDatabaseConfig>();
            services.AddSingleton<IDatabaseConfig>(
                new TestDatabaseConfig(_container.GetConnectionString())
            );
        });
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var dbCreator = scope.ServiceProvider.GetRequiredService<IDbCreator>();
        using var db = dbCreator.Create();
        await db.Database.ExecuteSqlRawAsync(
            """
            TRUNCATE TABLE "Attachments", "Comments", "RefreshToken",
                           "TasksInfo", "ProjectsInfo", "UsersInfo"
            RESTART IDENTITY CASCADE;
            """
        );
    }

    public new async Task DisposeAsync()
    {
        await _container.DisposeAsync();
        await base.DisposeAsync();
    }
}
