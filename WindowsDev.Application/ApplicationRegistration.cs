using Microsoft.Extensions.DependencyInjection;
using WindowsDev.Application.Common.Utils.DebounceService;
using WindowsDev.Application.Identity;
using WindowsDev.Application.Identity.Authentication;
using WindowsDev.Application.Identity.PasswordChanger;
using WindowsDev.Application.Identity.PasswordRecovery;
using WindowsDev.Application.Identity.Registration;
using WindowsDev.Application.Projects;
using WindowsDev.Application.Tasks;
using WindowsDev.Application.Tasks.Comment;
using WindowsDev.Application.Users;

namespace WindowsDev.Application;

public static class ApplicationRegistration
{
    public static IServiceCollection RegistrateApplication(this IServiceCollection services)
    {
        // Auth
        services.AddTransient<IAuthentication, Authentication>();
        services.AddTransient<IRegistration, Registration>();

        // Passwords
        services.AddTransient<IPasswordRecoveryService, PasswordRecoveryService>();
        services.AddTransient<IPasswordChanger, PasswordChanger>();

        // Projects
        services.AddTransient<IProjectService, ProjectService>();

        // Tasks
        services.AddTransient<ITaskService, TaskService>();
        services.AddTransient<ICommentService, CommentsService>();
        //services.AddTransient<IAttachmentService, AttachmentService>();

        // Profile
        services.AddTransient<IProfileService, ProfileService>();

        // Debounce
        services.AddTransient<IDebounceService, DebounceService>();

        return services;
    }
}