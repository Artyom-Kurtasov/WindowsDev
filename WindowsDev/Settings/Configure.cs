using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WindowsDev.Businnes.DataBase;
using WindowsDev.Businnes.Services.PasswordManager;
using WindowsDev.Infrastructure;
using WindowsDev.ViewModels;

namespace WindowsDev.Settings
{
    public class Configure
    {
        public void ConfigureServices(IServiceCollection services)
        {
            string connectionString = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=q29384756K5";
            services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString), ServiceLifetime.Transient);

            //PasswordManager
            services.AddTransient<PasswordHasher>();

            //commands
            services.AddTransient<RelayCommand>();

            //ViewModels
            services.AddSingleton<AuthorizationViewModel>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<RegistrationViewModel>();

            //Window
            services.AddSingleton<MainWindow>();
        }
    }
}
