using WindowsDev.Api.Attachment;
using WindowsDev.Api.Attachments;

namespace WindowsDev.Api;

public static class RegistrationApi
{
    public static IServiceCollection RegistrationApiServices(this IServiceCollection services)
    {
        services.AddSingleton<IAttachmentService, AttachmentService>();
        return services;
    }
}
