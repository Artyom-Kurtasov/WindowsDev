using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WindowsDev.Application.Identity.Cryptography;
using WindowsDev.Contracts.Api.Identity;
using WindowsDev.Domain.Entities;
using WindowsDev.Domain.Enums;
using WindowsDev.Infrastructure.Database.Interfaces;
using WindowsDev.IntegrationTests.Infrastructure;
using WindowsDev.Infrastructure.Security;

namespace WindowsDev.IntegrationTests;

public class AuthControllerTests : BaseIntegrationTest
{
    private const string TestLogin = "TestLogin";
    private const string TestUsername = "TestUsername";
    private const string TestPassword = "Password123!";
    private const string TestRecoveryCode = "123456";

    private readonly IDefaultHasher _defaultHasher;

    public AuthControllerTests(PostgreApiFactory factory) : base(factory)
    {
        _defaultHasher = new DefaultHasher();
    }

    private async Task SeedUserAsync(
        string login = TestLogin,
        string username = TestUsername,
        string password = TestPassword)
    {
        using var scope = Factory.Services.CreateScope();
        var dbCreator = scope.ServiceProvider.GetRequiredService<IDbCreator>();
        using var db = dbCreator.Create();

        var passwordSalt = _defaultHasher.GenerateSalt();
        var recoveryCodeSalt = _defaultHasher.GenerateSalt();
        var recoveryCodeHash = _defaultHasher.HashValue(TestRecoveryCode, recoveryCodeSalt).ToString("x16");
        var passwordHash = _defaultHasher.HashValue(password, passwordSalt).ToString("x16");

        var user = new UserInfo
        {
            Login = login,
            Username = username,
            PasswordHash = passwordHash,
            Salt = passwordSalt,
            HashMethod = HashMethod.Default,
            RecoveryCodeHash = recoveryCodeHash,
            RecoveryCodeSalt = recoveryCodeSalt
        };

        await db.UsersInfo.AddAsync(user);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task Register_WhenAllCorrect_ReturnCreated()
    {
        var request = new UserRegisterRequest
        {
            Login = "Test",
            Username = "Test",
            Password = TestPassword
        };

        var response = await Client.PostAsJsonAsync("api/auth/register", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<UserRegisterResponse>();
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body!.JwtToken));
        Assert.NotNull(body.RefreshToken);
    }

    [Fact]
    public async Task Register_WhenLoginTaken_ReturnConflict()
    {
        await SeedUserAsync(login: "TakenLogin", username: "OtherUsername");

        var request = new UserRegisterRequest
        {
            Login = "TakenLogin",
            Username = "NewUsername",
            Password = TestPassword
        };

        var response = await Client.PostAsJsonAsync("api/auth/register", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Register_WhenUsernameTaken_ReturnConflict()
    {
        await SeedUserAsync(login: "OtherLogin", username: "TakenUsername");

        var request = new UserRegisterRequest
        {
            Login = "NewLogin",
            Username = "TakenUsername",
            Password = TestPassword
        };

        var response = await Client.PostAsJsonAsync("api/auth/register", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Login_WhenAllCorrect_ReturnOk()
    {
        await SeedUserAsync();

        var request = new UserLoginRequest { Login = TestLogin, Password = TestPassword };

        var response = await Client.PostAsJsonAsync("api/auth/login", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<UserLoginResponse>();
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body!.AccessToken));
        Assert.NotNull(body.RefreshToken);
    }

    [Fact]
    public async Task Login_WhenPasswordWrong_ReturnUnauthorized()
    {
        await SeedUserAsync();

        var request = new UserLoginRequest { Login = TestLogin, Password = "WrongPassword!" };

        var response = await Client.PostAsJsonAsync("api/auth/login", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WhenUserNotFound_ReturnUnauthorized()
    {
        var request = new UserLoginRequest { Login = "NoSuchUser", Password = TestPassword };

        var response = await Client.PostAsJsonAsync("api/auth/login", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_AfterRegister_ReturnOk()
    {
        var registerRequest = new UserRegisterRequest
        {
            Login = "FlowLogin",
            Username = "FlowUsername",
            Password = TestPassword
        };
        var registerResponse = await Client.PostAsJsonAsync("api/auth/register", registerRequest);
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var loginRequest = new UserLoginRequest { Login = "FlowLogin", Password = TestPassword };
        var loginResponse = await Client.PostAsJsonAsync("api/auth/login", loginRequest);

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }

    [Fact]
    public async Task Refresh_WhenTokenValid_ReturnOk()
    {
        await SeedUserAsync();

        var loginRequest = new UserLoginRequest { Login = TestLogin, Password = TestPassword };
        var loginResponse = await Client.PostAsJsonAsync("api/auth/login", loginRequest);
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<UserLoginResponse>();
        Assert.NotNull(loginBody);

        var refreshRequest = new RefreshTokenRequest { RefreshToken = loginBody.RefreshToken.Token};
        var response = await Client.PostAsJsonAsync("api/auth/refresh", refreshRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<UserLoginResponse>();
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body!.AccessToken));
        Assert.NotNull(body.RefreshToken);
    }

    [Fact]
    public async Task Refresh_WhenTokenInvalid_ReturnUnauthorized()
    {
        var request = new RefreshTokenRequest { RefreshToken = "invalid-token" };

        var response = await Client.PostAsJsonAsync("api/auth/refresh", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_WhenTokenEmpty_ReturnUnauthorized()
    {
        var request = new RefreshTokenRequest { RefreshToken = string.Empty };

        var response = await Client.PostAsJsonAsync("api/auth/refresh", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task IsUsernameAvailable_WhenFree_ReturnTrue()
    {
        var request = new IsUsernameAvailableRequest ("FreeUsername");

        var response = await Client.PostAsJsonAsync("api/auth/isUsernameAvailable", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<IsUsernameAvailableResponse>();
        Assert.NotNull(body);
        Assert.True(body!.Result);
    }

    [Fact]
    public async Task IsUsernameAvailable_WhenTaken_ReturnFalse()
    {
        await SeedUserAsync();

        var request = new IsUsernameAvailableRequest (TestUsername);

        var response = await Client.PostAsJsonAsync("api/auth/isUsernameAvailable", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<IsUsernameAvailableResponse>();
        Assert.NotNull(body);
        Assert.False(body!.Result);
    }

    [Fact]
    public async Task IsLoginAvailable_WhenFree_ReturnTrue()
    {
        var request = new IsLoginAvailableRequest("FreeLogin" );

        var response = await Client.PostAsJsonAsync("api/auth/isLoginAvailable", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<IsLoginAvailableResponse>();
        Assert.NotNull(body);
        Assert.True(body!.Result);
    }

    [Fact]
    public async Task IsLoginAvailable_WhenTaken_ReturnFalse()
    {
        await SeedUserAsync();

        var request = new IsLoginAvailableRequest(TestLogin);

        var response = await Client.PostAsJsonAsync("api/auth/isLoginAvailable", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<IsLoginAvailableResponse>();
        Assert.NotNull(body);
        Assert.False(body!.Result);
    }
}