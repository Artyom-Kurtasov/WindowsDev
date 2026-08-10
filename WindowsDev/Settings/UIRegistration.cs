using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using WindowsDev.Application.Services.Localization;
using WindowsDev.Application.Services.TaskService.Attachment.FileService;
using WindowsDev.Factories;
using WindowsDev.Services.Dialogs;
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
    internal static class UIRegistration
    {
        public static IServiceCollection RegistrateUI(this IServiceCollection services)
        {
            // Window
            services.AddSingleton<MainWindow>();

            // ViewModels
            services.AddTransient<AuthorizationViewModel>();
            services.AddTransient<RegistrationViewModel>();

            services.AddTransient<ProfileViewModel>();
            services.AddTransient<ProjectsViewModel>();
            services.AddTransient<SettingsViewModel>();

            services.AddTransient<MainWindowViewModel>();

            services.AddTransient<ProjectViewModel>();
            services.AddTransient<CreateProjectDialogViewModel>();

            services.AddTransient<TaskViewModel>();

            services.AddTransient<EditTaskViewModel>();
            services.AddTransient<CreateTaskViewModel>();

            services.AddTransient<RecoveryCodeDialogViewModel>();
            services.AddTransient<FirstStepViewModel>();
            services.AddTransient<SecondStepViewModel>();
            services.AddTransient<ThirdStepViewModel>();

            // Services
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IDialogCoordinator, DialogCoordinator>();

            services.AddTransient<IFilePicker, FilePicker>();

            services.AddSingleton<ILanguageChanger, LanguageChanger>();

            services.AddSingleton<NavigationStore>();
            services.AddSingleton<INavigationService, NavigationService>();

            // Factories
            services.AddSingleton<IViewModelFactory, ViewModelFactory>();
            services.AddTransient<IRecoveryStepsFactory, RecoveryStepsFactory>();

            // Password Recovery Data
            services.AddSingleton<PasswordRecoveryData>();

            return services;
        }
    }
}
