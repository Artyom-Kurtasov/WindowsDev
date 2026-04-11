using MahApps.Metro.Controls.Dialogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WindowsDev.Business.DataBase;
using WindowsDev.Business.Services;
using WindowsDev.Business.Services.Localization;
using WindowsDev.Business.Services.PasswordManager;
using WindowsDev.Business.Services.ProjectService;
using WindowsDev.Business.Services.ProjectService.Interfaces;
using WindowsDev.Business.Services.Registration;
using WindowsDev.Business.Services.Registration.Validation;
using WindowsDev.Business.Services.TaskService;
using WindowsDev.Business.Services.TaskService.Attachment;
using WindowsDev.Business.Services.TaskService.Interfaces;
using WindowsDev.Business.Services.UserManager;
using WindowsDev.Commands.NavigationManager;
using WindowsDev.Commands.NavigationManager.Interfaces;
using WindowsDev.Domain.UsersAuthInfo;
using WindowsDev.Factories;
using WindowsDev.Factories.Interfaces;
using WindowsDev.ViewModels;

namespace WindowsDev.Settings
{
    /// <summary>
    /// Configures application services and dependency injection.
    /// </summary>
    public class Configure
    {
        /// <summary>
        /// Registers services and dependencies into the IServiceCollection.
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
            // Infrastructure
            services.AddSingleton<NavigationStore>();
            services.AddSingleton<IDialogCoordinator, DialogCoordinator>();

            // Factories
            services.AddSingleton<IViewModelFactory, ViewModelFactory>();

            // Navigation
            services.AddSingleton<INavigationService, NavigationService>();

            // Database
            services.AddSingleton<DbManager>();

            // Core Services
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
            services.AddSingleton<FileReader>();
            services.AddSingleton<FileWriter>();
            services.AddSingleton<LanguageChanger>();

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

            // DTOs
            services.AddTransient<TaskDTO>();
        }
    }
}

