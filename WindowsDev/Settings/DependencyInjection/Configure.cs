using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WindowsDev.Application.DatabaseInterfaces;
using WindowsDev.Application.RepositoriesInterfaces;
using WindowsDev.Application.Services.Authorization;
using WindowsDev.Application.Services.DebounceService;
using WindowsDev.Application.Services.Localization;
using WindowsDev.Application.Services.PasswordManager;
using WindowsDev.Application.Services.PasswordManager.Hasher;
using WindowsDev.Application.Services.PasswordManager.Hasher.Interfaces;
using WindowsDev.Application.Services.PasswordManager.PasswordRecovery;
using WindowsDev.Application.Services.Profile;
using WindowsDev.Application.Services.ProjectService;
using WindowsDev.Application.Services.Registration;
using WindowsDev.Application.Services.TaskService;
using WindowsDev.Application.Services.TaskService.Attachment;
using WindowsDev.Application.Services.TaskService.Attachment.FileService;
using WindowsDev.Application.Services.TaskService.Attachment.FileServiceInterfaces;
using WindowsDev.Application.Services.TaskService.Comment;
using WindowsDev.Application.Services.UserManager;
using WindowsDev.Converters;
using WindowsDev.Factories;
using WindowsDev.Infrastructure.Database;
using WindowsDev.Infrastructure.Database.Interfaces;
using WindowsDev.Infrastructure.FileOpener;
using WindowsDev.Infrastructure.Repositories;
using WindowsDev.Logging.Common;
using WindowsDev.Services.Dialogs;
using WindowsDev.Services.Dialogs.Interfaces;
using WindowsDev.Services.FilePicker;
using WindowsDev.Services.LanguageChanger;
using WindowsDev.Services.Navigation;
using WindowsDev.ViewModels.Auth.Dialogs.Factories;
using WindowsDev.ViewModels.Authorization;
using WindowsDev.ViewModels.Authorization.Dialogs;
using WindowsDev.ViewModels.Authorization.Dialogs.RecoverySteps;
using WindowsDev.ViewModels.Main;
using WindowsDev.ViewModels.Main.Tabs;
using WindowsDev.ViewModels.Project;
using WindowsDev.ViewModels.Projects.Dialogs;
using WindowsDev.ViewModels.Registration;
using WindowsDev.ViewModels.Tasks;
using WindowsDev.ViewModels.Tasks.Dialogs;

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

            services.AddTransient<IFileOpener, FileOpener>();
            services.AddTransient<IFilePicker, FilePicker>();
        }

        private static void ConfigureDatabase(IServiceCollection services)
        {
            services.AddSingleton<IDbCreator, DbCreator>();

            services.AddSingleton<IDatabaseConfig, DatabaseConfig>();

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
