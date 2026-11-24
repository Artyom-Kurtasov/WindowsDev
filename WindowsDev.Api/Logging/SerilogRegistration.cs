using Serilog;

namespace WindowsDev.Api.Logging;

public static class SerilogRegistration
{
    public static IServiceCollection RegistrateLogging(this IServiceCollection services)
    {
        var logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                "logs/app-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                shared: true)
            .CreateLogger();

        Log.Logger = logger;

        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddSerilog(logger, dispose: false);
        });

        return services;
    }
}
