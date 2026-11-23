using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using WindowsDev.Api.Controllers;
using WindowsDev.Api.ProblemDetails;
using WindowsDev.Application.Identity.PasswordRecovery;
using WindowsDev.Application.Primitives;
using WindowsDev.Contracts.Api.PasswordRecovery;
using WindowsDev.Domain.Entities;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;
using Xunit;

namespace WindowsDev.Tests.Api;

public class PasswordRecoveryControllerTests
{
    private readonly Mock<IPasswordRecoveryService> _passwordRecoveryServiceMock;
    private readonly Mock<ILogger<PasswordRecoveryController>> _loggerMock;

    public PasswordRecoveryControllerTests()
    {
        _passwordRecoveryServiceMock = new Mock<IPasswordRecoveryService>();
        _loggerMock = new Mock<ILogger<PasswordRecoveryController>>();
    }

    private PasswordRecoveryController CreateController()
    {
        var controller = new PasswordRecoveryController(
            _passwordRecoveryServiceMock.Object,
            _loggerMock.Object
        );

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext(),
        };

        return controller;
    }

    [Fact]
    public async Task IsRecoveryCodeCorrectAsync_WhenSuccess_ReturnsOkWithResponse()
    {
        var request = new VerifyRecoveryCodeRequest { login = "test", recoveryCode = 123456 };

        _passwordRecoveryServiceMock
            .Setup(x => x.IsRecoverCodeCorrectAsync(request.recoveryCode, request.login))
            .ReturnsAsync(Result<bool>.Success(true));

        var controller = CreateController();

        var result = await controller.IsRecoveryCodeCorrectAsync(request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<VerifyRecoveryCodeResponse>(ok.Value);
        Assert.True(response.IsValid);
    }

    [Fact]
    public async Task IsRecoveryCodeCorrectAsync_WhenInvalidCode_ReturnsUnprocessableEntity()
    {
        var request = new VerifyRecoveryCodeRequest { login = "test", recoveryCode = 999999 };

        _passwordRecoveryServiceMock
            .Setup(x => x.IsRecoverCodeCorrectAsync(request.recoveryCode, request.login))
            .ReturnsAsync(Result<bool>.Failure(PasswordRecoveryErrors.InvalidRecoveryCode));

        var controller = CreateController();

        var result = await controller.IsRecoveryCodeCorrectAsync(request);

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, objectResult.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(PasswordRecoveryProblemDetails.InvalidRecoveryCode, problem.Title);
    }

    [Fact]
    public async Task IsRecoveryCodeCorrectAsync_WhenUnexpectedError_ReturnsInternalServerError()
    {
        var request = new VerifyRecoveryCodeRequest { login = "test", recoveryCode = 123456 };

        _passwordRecoveryServiceMock
            .Setup(x => x.IsRecoverCodeCorrectAsync(request.recoveryCode, request.login))
            .ReturnsAsync(Result<bool>.Failure(CommonErrors.UnexpectedError));

        var controller = CreateController();

        var result = await controller.IsRecoveryCodeCorrectAsync(request);

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(PasswordRecoveryProblemDetails.UnexpectedError, problem.Title);
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenSuccess_ReturnsOkWithResponse()
    {
        var request = new ResetPasswordRequest { Login = "test", Password = "NewPassword123!" };
        var recoveryCode = 123456;
        var refreshToken = new UserRefreshToken { Id = 1, Token = "token", ExpiresAt = DateTime.UtcNow };

        _passwordRecoveryServiceMock
            .Setup(x => x.ChangePasswordAsync(request.Login, request.Password))
            .ReturnsAsync(Result<(int, string, UserRefreshToken)>.Success((recoveryCode, "jwt-token", refreshToken)));

        var controller = CreateController();

        var result = await controller.ChangePasswordAsync(request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ResetPasswordResponse>(ok.Value);
        Assert.Equal(recoveryCode, response.RecoveryCode);
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenUserNotFound_ReturnsBadRequest()
    {
        var request = new ResetPasswordRequest { Login = "unknown", Password = "NewPassword123!" };

        _passwordRecoveryServiceMock
            .Setup(x => x.ChangePasswordAsync(request.Login, request.Password))
            .ReturnsAsync(Result<(int, string, UserRefreshToken)>.Failure(CommonErrors.UserNotFound));

        var controller = CreateController();

        var result = await controller.ChangePasswordAsync(request);

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(PasswordRecoveryProblemDetails.UserNotFound, problem.Title);
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenUnexpectedError_ReturnsInternalServerError()
    {
        var request = new ResetPasswordRequest { Login = "test", Password = "NewPassword123!" };

        _passwordRecoveryServiceMock
            .Setup(x => x.ChangePasswordAsync(request.Login, request.Password))
            .ReturnsAsync(Result<(int, string, UserRefreshToken)>.Failure(CommonErrors.UnexpectedError));

        var controller = CreateController();

        var result = await controller.ChangePasswordAsync(request);

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(PasswordRecoveryProblemDetails.PasswordChangeFailed, problem.Title);
    }

    [Fact]
    public async Task IsUserExist_WhenCalled_ReturnsOkWithResponse()
    {
        var request = new IsUserExistRequest { Login = "test" };

        _passwordRecoveryServiceMock
            .Setup(x => x.IsUserExistAsync(request.Login))
            .ReturnsAsync(Result<bool>.Success(true));

        var controller = CreateController();

        var result = await controller.IsUserExist(request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<IsUserExistResponse>(ok.Value);
        Assert.True(response.IsExist);
    }

    [Fact]
    public async Task IsUserExist_WhenUserDoesNotExist_ReturnsOkWithFalse()
    {
        var request = new IsUserExistRequest { Login = "unknown" };

        _passwordRecoveryServiceMock
            .Setup(x => x.IsUserExistAsync(request.Login))
            .ReturnsAsync(Result<bool>.Success(false));

        var controller = CreateController();

        var result = await controller.IsUserExist(request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<IsUserExistResponse>(ok.Value);
        Assert.False(response.IsExist);
    }
}