using Microsoft.Extensions.DependencyInjection;
using WindowsDev.ApiClient.AttachmentClient;
using WindowsDev.ApiClient.AuthClient;
using WindowsDev.ApiClient.CommentClient;
using WindowsDev.ApiClient.Mappings;
using WindowsDev.ApiClient.PasswordRecovery;
using WindowsDev.ApiClient.ProfileClient;
using WindowsDev.ApiClient.ProjectsClient;
using WindowsDev.ApiClient.TaskClient;

namespace WindowsDev.ApiClient;

public static class ApiClientRegistration
{
    public static IServiceCollection RegistrateApiClient(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<ApiClientMappingProfile>();
        });

        services.AddTransient<TokenRefreshHandler>();

        services
            .AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7145");
                client.Timeout = TimeSpan.FromSeconds(15);
            });
        services
            .AddHttpClient<IProjectsApiClient, ProjectsApiClient>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7145");
                client.Timeout = TimeSpan.FromSeconds(15);
            })
            .AddHttpMessageHandler<TokenRefreshHandler>();
        services
            .AddHttpClient<ITaskApiClient, TaskApiClient>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7145");
                client.Timeout = TimeSpan.FromSeconds(15);
            })
            .AddHttpMessageHandler<TokenRefreshHandler>();
        services
            .AddHttpClient<IProfileApiClient, ProfileApiClient>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7145");
                client.Timeout = TimeSpan.FromSeconds(15);
            })
            .AddHttpMessageHandler<TokenRefreshHandler>();
        services
            .AddHttpClient<ICommentApiClient, CommentApiClient>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7145");
                client.Timeout = TimeSpan.FromSeconds(15);
            })
            .AddHttpMessageHandler<TokenRefreshHandler>();
        services
            .AddHttpClient<IAttachmentApiClient, AttachmentApiClient>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7145");
                client.Timeout = TimeSpan.FromSeconds(15);
            })
            .AddHttpMessageHandler<TokenRefreshHandler>();
        services
            .AddHttpClient<IPasswordRecoveryApiClient, PasswordRecoveryApiClient>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7145");
                client.Timeout = TimeSpan.FromSeconds(15);
            })
            .AddHttpMessageHandler<TokenRefreshHandler>();

        return services;
    }
}
