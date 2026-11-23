using System.Net;
using System.Net.Http.Json;
using AutoMapper;
using Moq;
using Moq.Protected;
using WindowsDev.ApiClient.PasswordRecovery;
using WindowsDev.Application.Primitives;
using WindowsDev.Contracts.Api.Common;
using WindowsDev.Contracts.Api.PasswordRecovery;
using WindowsDev.Contracts.UI.PasswordRecovery;
using Xunit;

namespace WindowsDev.Tests.ApiClient;

public class PasswordRecoveryApiClientTests
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<HttpMessageHandler> _handlerMock;
    private readonly HttpClient _httpClient;
    private readonly PasswordRecoveryApiClient _client;

    public PasswordRecoveryApiClientTests()
    {
        _mapperMock = new Mock<IMapper>();
        _handlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost/"),
        };
        _client = new PasswordRecoveryApiClient(_httpClient, _mapperMock.Object);
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
    public async Task IsRecoveryCodeCorrectAsync_WhenSuccess_ReturnsSuccessResult()
    {
        var form = new VerifyRecoveryCodeForm(123456, "login");
        var apiRequest = new VerifyRecoveryCodeRequest { recoveryCode = 123456, login = "login" };
        var responseDto = new VerifyRecoveryCodeResponse { IsValid = true };
        var info = new VerifyRecoveryCodeInfo(true);

        _mapperMock.Setup(x => x.Map<VerifyRecoveryCodeRequest>(form)).Returns(apiRequest);
        SetupHttpResponse(HttpStatusCode.OK, responseDto);
        _mapperMock.Setup(x => x.Map<VerifyRecoveryCodeInfo>(It.IsAny<VerifyRecoveryCodeResponse>())).Returns(info);

        var result = await _client.IsRecoveryCodeCorrectAsync(form, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Same(info, result.Value);
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenSuccess_ReturnsSuccessResult()
    {
        var form = new ResetPasswordForm("login", "newPassword");
        var apiRequest = new ResetPasswordRequest { Login = "login", Password = "newPassword" };
        var responseDto = new ResetPasswordResponse { RecoveryCode = 123 };
        var info = new ResetPasswordInfo(123);

        _mapperMock.Setup(x => x.Map<ResetPasswordRequest>(form)).Returns(apiRequest);
        SetupHttpResponse(HttpStatusCode.OK, responseDto);
        _mapperMock.Setup(x => x.Map<ResetPasswordInfo>(It.IsAny<ResetPasswordResponse>())).Returns(info);

        var result = await _client.ChangePasswordAsync(form, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Same(info, result.Value);
    }

    [Fact]
    public async Task IsUserExistAsync_WhenSuccess_ReturnsSuccessResult()
    {
        var form = new IsUserExistForm("login");
        var apiRequest = new IsUserExistRequest { Login = "login" };
        var responseDto = new IsUserExistResponse { IsExist = true };
        var info = new IsUserExistInfo(true);

        _mapperMock.Setup(x => x.Map<IsUserExistRequest>(form)).Returns(apiRequest);
        SetupHttpResponse(HttpStatusCode.OK, responseDto);
        _mapperMock.Setup(x => x.Map<IsUserExistInfo>(It.IsAny<IsUserExistResponse>())).Returns(info);

        var result = await _client.IsUserExistAsync(form, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Same(info, result.Value);
    }
}