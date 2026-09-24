using Microsoft.Extensions.DependencyInjection;
using WindowsDev.Application.Database;
using WindowsDev.Application.Identity;
using WindowsDev.Application.Projects;
using WindowsDev.Application.Tasks;
using WindowsDev.Application.Tasks.Attachment;
using WindowsDev.Application.Tasks.Comment;
using WindowsDev.Application.Users;
using WindowsDev.Infrastructure.Database;
using WindowsDev.Infrastructure.Database.Interfaces;
using WindowsDev.Infrastructure.JWT;
using WindowsDev.Infrastructure.Repositories;
using WindowsDev.Infrastructure.Security;
using WindowsDev.Infrastructure.User;

namespace WindowsDev.Infrastructure;

public static class InfrastructureRegistration
{
    public static IServiceCollection RegistrateInfrastructure(this IServiceCollection services)
    {
        // File
        services.AddTransient<IFileOpener, FileOpener.FileOpener>();

        // Repositories
        services.AddTransient<IUserRepository, UserRepository>();
        services.AddTransient<IProjectRepository, ProjectRepository>();
        services.AddTransient<ITaskRepository, TaskRepository>();
        services.AddTransient<IAttachmentRepository, AttachmentRepository>();
        services.AddTransient<ICommentRepository, CommentRepository>();
        services.AddTransient<IRefreshTokenRepository, RefreshTokenRepository>();

        // Database
        services.AddSingleton<IDbCreator, DbCreator>();
        services.AddSingleton<IDatabaseConfig, DatabaseConfig>();
        services.AddTransient<IDbHealthChecker, DbHealthChecker>();

        // Hasher
        services.AddTransient<IDefaultHasher, DefaultHasher>();
        services.AddTransient<ISimpleHasher, SimpleHasher>();
        services.AddTransient<IHasherFactory, HasherFactory>();

        services.AddTransient<IUserSession, UserSession>();
        services.AddHttpContextAccessor();

        services.AddSingleton<ISecureTokenStorage, SecureTokenStorage>();

        return services;
    }
}
