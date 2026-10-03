using Microsoft.Extensions.DependencyInjection;
using WindowsDev.Infrastructure.Logging;

namespace WindowsDev.Settings;

internal class Configure
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.RegistrateLogging();
        services.RegistrateUI();
    }
}