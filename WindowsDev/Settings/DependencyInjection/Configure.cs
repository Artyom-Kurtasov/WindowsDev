using Microsoft.Extensions.DependencyInjection;
using WindowsDev.ApiClient;
using WindowsDev.Logging;

namespace WindowsDev.Settings;

internal class Configure
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.RegistrateLogging();
        services.RegistrateUI();
        services.RegistrateApiClient();
    }
}