using Microsoft.Extensions.DependencyInjection;
using WindowsDev.Application.Common.Events;

namespace WindowsDev.Application;

public static class RegistrateCommonEvents
{
    public static IServiceCollection RegistrateEvents(this IServiceCollection services)
    {
        services.AddSingleton<ISessionEvent, SessionEvent>();
        return services;
    }
}
