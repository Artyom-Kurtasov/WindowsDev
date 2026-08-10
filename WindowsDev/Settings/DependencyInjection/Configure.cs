using Microsoft.Extensions.DependencyInjection;
using WindowsDev.Application;
using WindowsDev.Infrastructure;
using WindowsDev.Infrastructure.Logging;

namespace WindowsDev.Settings
{
    internal class Configure
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.RegistrateLogging();
            services.RegistrateUI();
            services.RegistrateInfrastructure();
            services.RegistrateApplication();

        }
    }
}
