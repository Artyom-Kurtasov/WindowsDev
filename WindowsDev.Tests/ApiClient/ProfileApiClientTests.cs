using AutoMapper;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;
using WindowsDev.ApiClient.ProfileClient;
using WindowsDev.Application.Identity.Cryptography;
using WindowsDev.Contracts.Api.Profiles;
using WindowsDev.Contracts.UI.Profiles;

namespace WindowsDev.Tests.ApiClient;

public class ProfileApiClientTests
{
    private readonly Mock<ISecureTokenStorage> _tokenStorageMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<HttpMessageHandler> _handlerMock;
    private readonly HttpClient _httpClient;
    private readonly ProfileApiClient _client;

    public ProfileApiClientTests()
    {
        _tokenStorageMock = new Mock<ISecureTokenStorage>();
        _mapperMock = new Mock<IMapper>();
        _handlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost/"),
        };
        _client = new ProfileApiClient(_httpClient, _tokenStorageMock.Object, _mapperMock.Object);
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
    public async Task ChangePasswordAsync_WhenSuccess_ReturnsSuccessResult()
    {
        var form = new ChangePasswordForm("curr", "new", "conf", "login");
        var apiRequest = new ChangePasswordRequest
        {
            CurrentPassword = "curr",
            NewPassword = "new",
            ConfirmPassword = "conf"
        };
        var responseDto = new ChangePasswordResponse { Token = "token", RecoveryCode = 123 };
        var info = new ChangePasswordInfo(123, "token");

        _mapperMock.Setup(x => x.Map<ChangePasswordRequest>(form)).Returns(apiRequest);
        SetupHttpResponse(HttpStatusCode.OK, responseDto);
        _mapperMock.Setup(x => x.Map<ChangePasswordInfo>(It.IsAny<ChangePasswordResponse>())).Returns(info);

        var result = await _client.ChangePasswordAsync(form, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Same(info, result.Value);
    }

    [Fact]
    public async Task ChangeUsernameAsync_WhenSuccess_ReturnsSuccessResult()
    {
        var form = new ChangeUsernameForm("curr", "new");
        var apiRequest = new ChangeUsernameRequest
        {
            CurrentUsername = "curr",
            NewUsername = "new"
        };
        var responseDto = new ChangeUsernameResponse { AccessToken = "access" };
        var info = new ChangeUsernameInfo("access");

        _mapperMock.Setup(x => x.Map<ChangeUsernameRequest>(form)).Returns(apiRequest);
        SetupHttpResponse(HttpStatusCode.OK, responseDto);
        _mapperMock.Setup(x => x.Map<ChangeUsernameInfo>(It.IsAny<ChangeUsernameResponse>())).Returns(info);

        var result = await _client.ChangeUsernameAsync(form, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Same(info, result.Value);
    }

    [Fact]
    public async Task GetAsync_WhenSuccess_ReturnsSuccessResult()
    {
        var responseDto = new GetProfileResponse { Id = 1, Login = "login", Username = "username" };
        var info = new ProfileInfo(1, "login", "username");

        SetupHttpResponse(HttpStatusCode.OK, responseDto);
        _mapperMock.Setup(x => x.Map<ProfileInfo>(It.IsAny<GetProfileResponse>())).Returns(info);

        var result = await _client.GetAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Same(info, result.Value);
    }
}