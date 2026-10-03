using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using WindowsDev.ApiClients.AttachmentClient;
using WindowsDev.ApiClients.AuthClient;
using WindowsDev.ApiClients.CommentClient;
using WindowsDev.ApiClients.PasswordRecoveryClient;
using WindowsDev.ApiClients.ProfileClient;
using WindowsDev.ApiClients.ProjectsClient;
using WindowsDev.ApiClients.TaskClient;
using WindowsDev.Application.Common.Utils.Localization;
using WindowsDev.Application.Identity;
using WindowsDev.Factories;
using WindowsDev.Services;
using WindowsDev.Services.Dialogs;
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

namespace WindowsDev.Settings;

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

        services.AddSingleton<ILanguageChanger, LanguageChanger>();

        services.AddSingleton<NavigationStore>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IDialogContextProvider, DialogContextProvider>();

        services.AddSingleton<ISecureTokenStorage, SecureTokenStorage>();

        // Factories
        services.AddSingleton<IViewModelFactory, ViewModelFactory>();
        services.AddTransient<IRecoveryStepsFactory, RecoveryStepsFactory>();

        // Password Recovery Data
        services.AddSingleton<PasswordRecoveryData>();

        services.AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
        {
            client.BaseAddress = new Uri("http://192.168.0.133:5004");
            client.Timeout = TimeSpan.FromSeconds(15);
        });
        services.AddHttpClient<IProjectsApiClient, ProjectsApiClient>(client =>
        {
            client.BaseAddress = new Uri("http://192.168.0.133:5004");
            client.Timeout = TimeSpan.FromSeconds(15);
        });
        services.AddHttpClient<ITaskApiClient, TaskApiClient>(client =>
        {
            client.BaseAddress = new Uri("http://192.168.0.133:5004");
            client.Timeout = TimeSpan.FromSeconds(15);
        });
        services.AddHttpClient<IProfileApiClient, ProfileApiClient>(client =>
        {
            client.BaseAddress = new Uri("http://192.168.0.133:5004");
            client.Timeout = TimeSpan.FromSeconds(15);
        });
        services.AddHttpClient<ICommentApiClient, CommentApiClient>(client =>
        {
            client.BaseAddress = new Uri("http://192.168.0.133:5004");
            client.Timeout = TimeSpan.FromSeconds(15);
        });
        services.AddHttpClient<IAttachmentApiClient, AttachmentApiClient>(client =>
        {
            client.BaseAddress = new Uri("http://192.168.0.133:5004");
            client.Timeout = TimeSpan.FromSeconds(15);
        });
        services.AddHttpClient<IPasswordRecoveryApiClient, PasswordRecoveryApiClient>(client =>
        {
            client.BaseAddress = new Uri("http://192.168.0.133:5004");
            client.Timeout = TimeSpan.FromSeconds(15);
        });


        return services;
    }
}