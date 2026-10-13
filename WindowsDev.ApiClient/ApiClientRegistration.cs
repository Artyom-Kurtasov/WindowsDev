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
        services.AddAutoMapper(cfg => { cfg.AddProfile<ApiClientMappingProfile>(); });

        services.AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
        {
            client.BaseAddress = new Uri("http://192.168.0.133:5004");
            client.Timeout = TimeSpan.FromSeconds(15);
        });
        services.AddHttpClient<IProjectsApiClient, ProjectsApiClient>(client =>
        {
            client.BaseAddress = new Uri("http://192.168.0.133:5004");
            client.Timeout = TimeSpan.FromSeconds(15);
        });
        services.AddHttpClient<ITaskApiClient, TaskApiClient>(client =>
        {
            client.BaseAddress = new Uri("http://192.168.0.133:5004");
            client.Timeout = TimeSpan.FromSeconds(15);
        });
        services.AddHttpClient<IProfileApiClient, ProfileApiClient>(client =>
        {
            client.BaseAddress = new Uri("http://192.168.0.133:5004");
            client.Timeout = TimeSpan.FromSeconds(15);
        });
        services.AddHttpClient<ICommentApiClient, CommentApiClient>(client =>
        {
            client.BaseAddress = new Uri("http://192.168.0.133:5004");
            client.Timeout = TimeSpan.FromSeconds(15);
        });
        services.AddHttpClient<IAttachmentApiClient, AttachmentApiClient>(client =>
        {
            client.BaseAddress = new Uri("http://192.168.0.133:5004");
            client.Timeout = TimeSpan.FromSeconds(15);
        });
        services.AddHttpClient<IPasswordRecoveryApiClient, PasswordRecoveryApiClient>(client =>
        {
            client.BaseAddress = new Uri("http://192.168.0.133:5004");
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        return services;
    }
}
