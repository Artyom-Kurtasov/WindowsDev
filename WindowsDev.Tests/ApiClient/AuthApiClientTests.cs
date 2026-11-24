using AutoMapper;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;
using WindowsDev.ApiClient.AuthClient;
using WindowsDev.Contracts.Api.Common;
using WindowsDev.Contracts.Api.Identity;
using WindowsDev.Contracts.UI.Identity;
using WindowsDev.Domain.Entities;

namespace WindowsDev.Tests.ApiClient;

public class AuthApiClientTests
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<HttpMessageHandler> _handlerMock;
    private readonly HttpClient _httpClient;
    private readonly AuthApiClient _client;

    public AuthApiClientTests()
    {
        _mapperMock = new Mock<IMapper>();
        _handlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost/"),
        };
        _client = new AuthApiClient(_httpClient, _mapperMock.Object);
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, object? content = null)
    {
        var response = new HttpResponseMessage(statusCode);
        if (content != null)
        {
            response.Content = JsonContent.Create(content);
        }

        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
    }

    [Fact]
    public async Task RefreshAsync_WhenSuccess_ReturnsSuccessResult()
    {
        var request = new RefreshTokenRequest { RefreshToken = new UserRefreshToken() };
        var responseDto = new UserLoginResponse { AccessToken = "access", RefreshToken = new UserRefreshToken() };
        var tokens = new TokensInfo("access", new UserRefreshToken());

        SetupHttpResponse(HttpStatusCode.OK, responseDto);
        _mapperMock.Setup(x => x.Map<TokensInfo>(It.IsAny<UserLoginResponse>())).Returns(tokens);

        var result = await _client.RefreshAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Same(tokens, result.Value);
    }

    [Fact]
    public async Task RefreshAsync_WhenFailure_ReturnsFailureResult()
    {
        var request = new RefreshTokenRequest { RefreshToken = new UserRefreshToken() };
        var problem = new ProblemDetails { Title = "Error" };

        SetupHttpResponse(HttpStatusCode.Unauthorized, problem);

        var result = await _client.RefreshAsync(request);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task LoginAsync_WhenSuccess_ReturnsSuccessResult()
    {
        var request = new LoginForm("login", "password");
        var apiRequest = new UserLoginRequest();
        var responseDto = new UserLoginResponse { AccessToken = "access", RefreshToken = new UserRefreshToken() };
        var tokens = new TokensInfo("access", new UserRefreshToken());

        _mapperMock.Setup(x => x.Map<UserLoginRequest>(request)).Returns(apiRequest);
        SetupHttpResponse(HttpStatusCode.OK, responseDto);
        _mapperMock.Setup(x => x.Map<TokensInfo>(It.IsAny<UserLoginResponse>())).Returns(tokens);

        var result = await _client.LoginAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Same(tokens, result.Value);
    }

    [Fact]
    public async Task LoginAsync_WhenFailure_ReturnsFailureResult()
    {
        var request = new LoginForm("login", "password");
        var apiRequest = new UserLoginRequest();
        var problem = new ProblemDetails { Title = "Error" };

        _mapperMock.Setup(x => x.Map<UserLoginRequest>(request)).Returns(apiRequest);
        SetupHttpResponse(HttpStatusCode.BadRequest, problem);

        var result = await _client.LoginAsync(request);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task RegisterAsync_WhenSuccess_ReturnsSuccessResult()
    {
        var request = new RegisterForm("login", "password", "username");
        var apiRequest = new UserRegisterRequest();
        var responseDto = new UserRegisterResponse(123, "jwt", new UserRefreshToken());
        var info = new RegisterInfo(123, "jwt", new UserRefreshToken());

        _mapperMock.Setup(x => x.Map<UserRegisterRequest>(request)).Returns(apiRequest);
        SetupHttpResponse(HttpStatusCode.OK, responseDto);
        _mapperMock.Setup(x => x.Map<RegisterInfo>(It.IsAny<UserRegisterResponse>())).Returns(info);

        var result = await _client.RegisterAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(info.RecoveryCode, result.Value.RecoveryCode);
    }

    [Fact]
    public async Task IsUsernameAvailableAsync_WhenSuccess_ReturnsSuccessResult()
    {
        var form = new UsernameAvailabilityForm("username");
        var apiRequest = new IsUsernameAvailableRequest("username");
        var responseDto = new IsUsernameAvailableResponse(true);
        var info = new AvailabilityInfo(true);

        _mapperMock.Setup(x => x.Map<IsUsernameAvailableRequest>(form)).Returns(apiRequest);
        SetupHttpResponse(HttpStatusCode.OK, responseDto);
        _mapperMock.Setup(x => x.Map<AvailabilityInfo>(It.IsAny<IsUsernameAvailableResponse>())).Returns(info);

        var result = await _client.IsUsernameAvailableAsync(form);

        Assert.True(result.IsSuccess);
        Assert.Same(info, result.Value);
    }

    [Fact]
    public async Task IsLoginAvailableAsync_WhenSuccess_ReturnsSuccessResult()
    {
        var form = new LoginAvailabilityForm("login");
        var apiRequest = new IsLoginAvailableRequest("login");
        var responseDto = new IsLoginAvailableResponse(true);
        var info = new AvailabilityInfo(true);

        _mapperMock.Setup(x => x.Map<IsLoginAvailableRequest>(form)).Returns(apiRequest);
        SetupHttpResponse(HttpStatusCode.OK, responseDto);
        _mapperMock.Setup(x => x.Map<AvailabilityInfo>(It.IsAny<IsLoginAvailableResponse>())).Returns(info);

        var result = await _client.IsLoginAvailableAsync(form);

        Assert.True(result.IsSuccess);
        Assert.Same(info, result.Value);
    }
}