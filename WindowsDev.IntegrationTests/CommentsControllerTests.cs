using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using WindowsDev.Application.Identity.Cryptography;
using WindowsDev.Contracts.Api.Comments;
using WindowsDev.Contracts.Api.Identity;
using WindowsDev.Domain.Entities;
using WindowsDev.Domain.Enums;
using WindowsDev.Infrastructure.Database.Interfaces;
using WindowsDev.Infrastructure.Security;
using WindowsDev.IntegrationTests.Infrastructure;
using TaskStatus = WindowsDev.Domain.Enums.TaskStatus;

namespace WindowsDev.IntegrationTests;

public class CommentsControllerTests : BaseIntegrationTest
{
    private const string TestLogin = "CommentsTestLogin";
    private const string TestUsername = "CommentsTestUsername";
    private const string TestPassword = "Password123!";

    private readonly IDefaultHasher _defaultHasher;

    public CommentsControllerTests(PostgreApiFactory factory)
        : base(factory)
    {
        _defaultHasher = new DefaultHasher();
    }

    private async Task<int> SeedUserAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var dbCreator = scope.ServiceProvider.GetRequiredService<IDbCreator>();
        using var db = dbCreator.Create();

        var passwordSalt = _defaultHasher.GenerateSalt();
        var recoveryCodeSalt = _defaultHasher.GenerateSalt();
        var recoveryCodeHash = _defaultHasher.HashValue("123456", recoveryCodeSalt).ToString("x16");
        var passwordHash = _defaultHasher.HashValue(TestPassword, passwordSalt).ToString("x16");

        var user = new UserInfo
        {
            Login = TestLogin,
            Username = TestUsername,
            PasswordHash = passwordHash,
            Salt = passwordSalt,
            HashMethod = HashMethod.Default,
            RecoveryCodeHash = recoveryCodeHash,
            RecoveryCodeSalt = recoveryCodeSalt,
        };

        await db.UsersInfo.AddAsync(user);
        await db.SaveChangesAsync();

        return user.Id;
    }

    private async Task<int> SeedProjectAsync(int userId, string name = "Test Project")
    {
        using var scope = Factory.Services.CreateScope();
        var dbCreator = scope.ServiceProvider.GetRequiredService<IDbCreator>();
        using var db = dbCreator.Create();

        var project = new ProjectsInfo
        {
            Name = name,
            Description = "Test Description",
            CreatedAt = DateTime.UtcNow,
            UserId = userId,
        };

        await db.ProjectsInfo.AddAsync(project);
        await db.SaveChangesAsync();

        return project.Id;
    }

    private async Task<int> SeedTaskAsync(int projectId, string taskName = "Test Task")
    {
        using var scope = Factory.Services.CreateScope();
        var dbCreator = scope.ServiceProvider.GetRequiredService<IDbCreator>();
        using var db = dbCreator.Create();

        var task = new TasksInfo
        {
            Name = taskName,
            Description = "Test Description",
            Priority = TaskPriority.Medium,
            Progress = 0,
            Status = TaskStatus.InProgress,
            ProjectId = projectId,
            CreatedAt = DateTime.UtcNow,
            Deadline = DateTime.UtcNow.AddDays(7),
        };

        await db.TasksInfo.AddAsync(task);
        await db.SaveChangesAsync();

        return task.Id;
    }

    private async Task<int> SeedCommentAsync(
        int taskId,
        string author,
        string text = "Seed comment"
    )
    {
        using var scope = Factory.Services.CreateScope();
        var dbCreator = scope.ServiceProvider.GetRequiredService<IDbCreator>();
        using var db = dbCreator.Create();

        var comment = new TaskComment
        {
            TaskId = taskId,
            Author = author,
            Text = text,
            CreatedAt = DateTime.UtcNow,
        };

        await db.Comments.AddAsync(comment);
        await db.SaveChangesAsync();

        return comment.Id;
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var loginRequest = new UserLoginRequest { Login = TestLogin, Password = TestPassword };
        var response = await Client.PostAsJsonAsync("api/auth/login", loginRequest);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<UserLoginResponse>();
        Assert.NotNull(body);

        var client = Factory.CreateClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            body!.AccessToken
        );

        return client;
    }

    [Fact]
    public async Task GetComment_WhenNotAuthenticated_ReturnUnauthorized()
    {
        var response = await Client.GetAsync("api/comments/1");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AddComment_WhenNotAuthenticated_ReturnUnauthorized()
    {
        var request = new AddCommentRequest { TaskId = 1, Text = "text" };
        var response = await Client.PostAsJsonAsync("api/comments", request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetComment_WhenExists_ReturnOk()
    {
        var userId = await SeedUserAsync();
        var projectId = await SeedProjectAsync(userId);
        var taskId = await SeedTaskAsync(projectId);
        var commentId = await SeedCommentAsync(taskId, TestUsername, "Hello");

        var client = await CreateAuthenticatedClientAsync();
        var response = await client.GetAsync($"api/comments/{commentId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<GetCommentResponse>();
        Assert.NotNull(body);
        Assert.Equal(commentId, body!.Id);
        Assert.Equal("Hello", body.Text);
    }

    [Fact]
    public async Task GetComment_WhenNotExists_ReturnNotFound()
    {
        await SeedUserAsync();

        var client = await CreateAuthenticatedClientAsync();
        var response = await client.GetAsync("api/comments/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetComments_WhenTaskHasComments_ReturnOkWithList()
    {
        var userId = await SeedUserAsync();
        var projectId = await SeedProjectAsync(userId);
        var taskId = await SeedTaskAsync(projectId);
        await SeedCommentAsync(taskId, TestUsername, "First");
        await SeedCommentAsync(taskId, TestUsername, "Second");

        var client = await CreateAuthenticatedClientAsync();
        var response = await client.GetAsync($"api/comments/task/{taskId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<List<GetCommentResponse>>();
        Assert.NotNull(body);
        Assert.Equal(2, body!.Count);
        Assert.Contains(body, c => c.Text == "First");
        Assert.Contains(body, c => c.Text == "Second");
    }

    [Fact]
    public async Task GetComments_WhenTaskHasNoComments_ReturnOkWithEmptyList()
    {
        var userId = await SeedUserAsync();
        var projectId = await SeedProjectAsync(userId);
        var taskId = await SeedTaskAsync(projectId);

        var client = await CreateAuthenticatedClientAsync();
        var response = await client.GetAsync($"api/comments/task/{taskId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<List<GetCommentResponse>>();
        Assert.NotNull(body);
        Assert.Empty(body!);
    }

    [Fact]
    public async Task AddComment_WhenAllCorrect_ReturnCreated()
    {
        var userId = await SeedUserAsync();
        var projectId = await SeedProjectAsync(userId);
        var taskId = await SeedTaskAsync(projectId);

        var client = await CreateAuthenticatedClientAsync();
        var request = new AddCommentRequest { TaskId = taskId, Text = "New comment" };

        var response = await client.PostAsJsonAsync("api/comments", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AddCommentResponse>();
        Assert.NotNull(body);
        Assert.True(body!.Id > 0);

        Assert.NotNull(response.Headers.Location);
        Assert.Contains($"api/Comments/{body.Id}", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task AddComment_ThenGet_ReturnsSameComment()
    {
        var userId = await SeedUserAsync();
        var projectId = await SeedProjectAsync(userId);
        var taskId = await SeedTaskAsync(projectId);

        var client = await CreateAuthenticatedClientAsync();
        var request = new AddCommentRequest { TaskId = taskId, Text = "Roundtrip" };

        var addResponse = await client.PostAsJsonAsync("api/comments", request);
        Assert.Equal(HttpStatusCode.Created, addResponse.StatusCode);
        var added = await addResponse.Content.ReadFromJsonAsync<AddCommentResponse>();
        Assert.NotNull(added);

        var getResponse = await client.GetAsync($"api/comments/{added!.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<GetCommentResponse>();
        Assert.NotNull(fetched);
        Assert.Equal(added.Id, fetched!.Id);
        Assert.Equal("Roundtrip", fetched.Text);
    }
}