using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WindowsDev.Business.DataBase;
using WindowsDev.Business.DataBase.Interfaces;
using WindowsDev.Business.Repositories;
using WindowsDev.Business.Repositories.Interfaces;
using WindowsDev.Business.Services.Authorization;
using WindowsDev.Business.Services.Authorization.Interfaces;
using WindowsDev.Business.Services.DebounceService;
using WindowsDev.Business.Services.Localization;
using WindowsDev.Business.Services.Localization.Interfaces;
using WindowsDev.Business.Services.Logger;
using WindowsDev.Business.Services.PasswordManager;
using WindowsDev.Business.Services.PasswordManager.Hasher;
using WindowsDev.Business.Services.PasswordManager.Hasher.Interfaces;
using WindowsDev.Business.Services.PasswordManager.Interfaces;
using WindowsDev.Business.Services.PasswordManager.PasswordRecovery;
using WindowsDev.Business.Services.PasswordManager.PasswordRecovery.Interfaces;
using WindowsDev.Business.Services.Profile;
using WindowsDev.Business.Services.Profile.Interfaces;
using WindowsDev.Business.Services.ProjectService;
using WindowsDev.Business.Services.ProjectService.Interfaces;
using WindowsDev.Business.Services.Registration;
using WindowsDev.Business.Services.Registration.Interfaces;
using WindowsDev.Business.Services.Registration.Validation.Converters;
using WindowsDev.Business.Services.TaskService;
using WindowsDev.Business.Services.TaskService.Attachment;
using WindowsDev.Business.Services.TaskService.Attachment.Interfaces;
using WindowsDev.Business.Services.TaskService.Comment;
using WindowsDev.Business.Services.TaskService.Comment.Interfaces;
using WindowsDev.Business.Services.TaskService.Interfaces;
using WindowsDev.Business.Services.UserManager;
using WindowsDev.Business.Services.UserManager.Interfaces;
using WindowsDev.Dialogs;
using WindowsDev.Dialogs.Interfaces;
using WindowsDev.Domain.PasswordRecoveryModels;
using WindowsDev.Factories;
using WindowsDev.Factories.Interfaces;
using WindowsDev.NavigationManager;
using WindowsDev.NavigationManager.Interfaces;
using WindowsDev.ViewModels.Auth;
using WindowsDev.ViewModels.Auth.Dialogs;
using WindowsDev.ViewModels.Auth.Dialogs.Factories;
using WindowsDev.ViewModels.Auth.Dialogs.RecoverySteps;
using WindowsDev.ViewModels.Main;
using WindowsDev.ViewModels.Main.Tabs;
using WindowsDev.ViewModels.Projects;
using WindowsDev.ViewModels.Projects.Dialogs;
using WindowsDev.ViewModels.Tasks;
using WindowsDev.ViewModels.Tasks.Dialog;

namespace WindowsDev.Settings
{
    public class Configure
    {
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureLogging(services);

            ConfigureInfrastructure(services);
            ConfigureDatabase(services);

            ConfigureRepositories(services);
            ConfigureBusinessServices(services);

            ConfigureViewModels(services);
            ConfigureWindows(services);
        }

        private static void ConfigureLogging(IServiceCollection services)
        {
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddProvider(new FileLoggerProvider("log.txt"));
            });

            services.AddSingleton(
                typeof(ILogger),
                sp => sp.GetRequiredService<ILoggerFactory>().CreateLogger("Default")
            );
        }

        private static void ConfigureInfrastructure(IServiceCollection services)
        {
            // Navigation
            services.AddSingleton<NavigationStore>();
            services.AddSingleton<INavigationService, NavigationService>();

            // Dialogs
            services.AddSingleton<IDialogCoordinator, DialogCoordinator>();
            services.AddSingleton<IDialogService, DialogService>();

            // Factories
            services.AddSingleton<IViewModelFactory, ViewModelFactory>();
            services.AddTransient<IRecoveryStepsFactory, RecoveryStepsFactory>();

            // Localization
            services.AddSingleton<ILanguageChanger, LanguageChanger>();

            // Converters
            services.AddSingleton<BoolToBrushConverter>();

            // Password Recovery Data
            services.AddSingleton<PasswordRecoveryData>();
        }

        private static void ConfigureDatabase(IServiceCollection services)
        {
            services.AddSingleton<IDbManager, DbManager>();

            services.AddTransient<IDbHealthChecker, DbHealthChecker>();
        }

        private static void ConfigureRepositories(IServiceCollection services)
        {
            services.AddTransient<IProjectRepository, ProjectRepository>();
            services.AddTransient<IUserRepository, UserRepository>();

            services.AddTransient<ITaskRepository, TaskRepository>();

            services.AddTransient<IAttachmentRepository, AttachmentRepository>();
            services.AddTransient<ICommentRepository, CommentRepository>();
        }

        private static void ConfigureBusinessServices(IServiceCollection services)
        {
            // Authorization
            services.AddTransient<IAuthorization, Authorization>();
            services.AddSingleton<IRegistration, Registration>();

            // User
            services.AddSingleton<ICurrentUserService, CurrentUserService>();

            // Passwords
            services.AddTransient<IHasherFactory, HasherFactory>();

            services.AddTransient<DefaultHasher>();
            services.AddTransient<SimpleHasher>();
            services.AddTransient<IPasswordRecoveryService, PasswordRecoveryService>();
            services.AddTransient<IPasswordChanger, PasswordChanger>();

            // Projects
            services.AddSingleton<IProjectService, ProjectService>();

            // Tasks
            services.AddTransient<ITaskService, TaskService>();

            services.AddSingleton<IAttacmentService, AttachmentService>();
            services.AddTransient<ICommentService, CommentsService>();

            // Profile
            services.AddSingleton<IProfileService, ProfileService>();

            // Debounce
            services.AddTransient<IDebounceService, DebounceService>();
        }

        private static void ConfigureViewModels(IServiceCollection services)
        {
            // Auth
            services.AddTransient<AuthorizationViewModel>();
            services.AddTransient<RegistrationViewModel>();

            // Main
            services.AddTransient<ProfileViewModel>();
            services.AddTransient<ProjectsViewModel>();
            services.AddTransient<SettingsViewModel>();

            services.AddTransient<MainWindowViewModel>();

            // Projects
            services.AddTransient<ProjectViewModel>();
            services.AddTransient<CreateProjectDialogViewModel>();

            // Tasks
            services.AddTransient<TaskViewModel>();

            services.AddTransient<EditTaskViewModel>();
            services.AddTransient<CreateTaskViewModel>();

            // Recovery Password
            services.AddTransient<RecoveryCodeDialogViewModel>();
            services.AddTransient<FirstStepViewModel>();
            services.AddTransient<SecondStepViewModel>();
            services.AddTransient<ThirdStepViewModel>();
        }

        private static void ConfigureWindows(IServiceCollection services)
        {
            services.AddSingleton<MainWindow>();
        }
    }
}
