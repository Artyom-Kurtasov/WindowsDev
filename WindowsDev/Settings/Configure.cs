using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WindowsDev.Businnes.DataBase;
using WindowsDev.Businnes.Services;
using WindowsDev.Businnes.Services.PasswordManager;
using WindowsDev.Commands.NavigationManager;
using WindowsDev.Commands.NavigationManager.Interfaces;
using WindowsDev.Domain.UsersAuthInfo;
using WindowsDev.Infrastructure;
using WindowsDev.ViewModels;

namespace WindowsDev.Settings
{
    public class Configure
    {
        public void ConfigureServices(IServiceCollection services)
        {
            string connectionString = "Host=localhost;Port=5432;Database=WindowsDev;Username=postgres;Password=q29384756";
            services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString), ServiceLifetime.Transient);

            services.AddSingleton<Registration>();
            //PasswordManager
            services.AddTransient<PasswordHasher>();

            //commands
            services.AddTransient<RelayCommand>();
            services.AddTransient<NavigationCommands>();
            services.AddSingleton<NavigationStore>();
            services.AddSingleton<INavigationService, NavigationService>();

            //ViewModels
            services.AddSingleton<AuthorizationViewModel>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<RegistrationViewModel>();

            //Window
            services.AddSingleton<MainWindow>();

            //Models
            services.AddSingleton<UserAuthInfo>();
        }
    }
}
