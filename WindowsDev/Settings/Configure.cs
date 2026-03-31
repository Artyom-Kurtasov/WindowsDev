using MahApps.Metro.Controls.Dialogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WindowsDev.Businnes.DataBase;
using WindowsDev.Businnes.Services;
using WindowsDev.Businnes.Services.PasswordManager;
using WindowsDev.Businnes.Services.ProjectService;
using WindowsDev.Businnes.Services.ProjectService.Interfaces;
using WindowsDev.Businnes.Services.Registration;
using WindowsDev.Businnes.Services.Registration.Validation;
using WindowsDev.Businnes.Services.TaskService;
using WindowsDev.Businnes.Services.TaskService.Interfaces;
using WindowsDev.Businnes.Services.UserManager;
using WindowsDev.Commands.NavigationManager;
using WindowsDev.Commands.NavigationManager.Interfaces;
using WindowsDev.Domain.UsersAuthInfo;
using WindowsDev.Factories;
using WindowsDev.Factories.Interfaces;
using WindowsDev.ViewModels;

namespace WindowsDev.Settings
{
    public class Configure
    {
        public void ConfigureServices(IServiceCollection services)
        {
            string connectionString =
                "Host=localhost;Port=5432;Database=WindowsDev;Username=postgres;Password=q29384756";

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));

            // Infrastructure
            services.AddSingleton<NavigationStore>();
            services.AddSingleton<IDialogCoordinator, DialogCoordinator>();

            // Factories
            services.AddSingleton<IViewModelFactory, ViewModelFactory>();

            // Navigation
            services.AddSingleton<INavigationService, NavigationService>();

            // Services
            services.AddSingleton<Registration>();
            services.AddTransient<PasswordHasher>();
            services.AddSingleton<DialogShowingService>();
            services.AddSingleton<SharedDataService>();
            services.AddSingleton<CurrentUserData>();
            services.AddTransient<AddComment>();
            services.AddSingleton<PasswordColorValidationConvert>();
            services.AddSingleton<LoginColorValidationConverter>();
            services.AddSingleton<UsernameColorValidationConverter>();
            services.AddSingleton<UserFieldValidator>();
            services.AddSingleton<CurrentUserService>();

            // Project services
            services.AddTransient<IProjectReader, ProjectReader>();
            services.AddTransient<IProjectWriter, ProjectWriter>();
            services.AddTransient<IProjectCreator, ProjectCreator>();
            services.AddTransient<IProjectLoader, ProjectLoader>();

            // Task services
            services.AddTransient<ITaskReader, TaskReader>();
            services.AddTransient<ITaskCreator, TaskCreator>();
            services.AddTransient<ITaskWriter, TaskWriter>();
            services.AddTransient<ITaskLoader, TaskLoader>();

            // Business services
            services.AddTransient<Authorization>();

            // ViewModels
            services.AddTransient<AuthorizationViewModel>();
            services.AddTransient<RegistrationViewModel>();
            services.AddTransient<ProjectViewModel>();
            services.AddTransient<DialogsViewModel>();
            services.AddTransient<TaskDialogViewModel>();

            services.AddSingleton<MainWindowViewModel>();

            // Windows
            services.AddSingleton<MainWindow>();

            //DTO
            services.AddTransient<TaskDTO>();
        }
    }
}