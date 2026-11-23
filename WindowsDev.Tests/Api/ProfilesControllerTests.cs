using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using WindowsDev.Api.Controllers;
using WindowsDev.Api.ProblemDetails;
using WindowsDev.Application.Identity;
using WindowsDev.Application.Identity.Cryptography;
using WindowsDev.Application.Primitives;
using WindowsDev.Application.Users;
using WindowsDev.Contracts.Api.Profiles;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;
using Xunit;

namespace WindowsDev.Tests.Api;

public class ProfilesControllerTests
{
    private readonly Mock<IProfileService> _profileServiceMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IUserSession> _userSessionMock;
    private readonly Mock<ILogger<ProfilesController>> _loggerMock;

    public ProfilesControllerTests()
    {
        _profileServiceMock = new Mock<IProfileService>();
        _jwtServiceMock = new Mock<IJwtService>();
        _userSessionMock = new Mock<IUserSession>();
        _loggerMock = new Mock<ILogger<ProfilesController>>();

        _userSessionMock.SetupGet(x => x.UserId).Returns(42);
        _userSessionMock.SetupGet(x => x.Login).Returns("test-login");
        _userSessionMock.SetupGet(x => x.Username).Returns("test-username");
    }

    private ProfilesController CreateController()
    {
        var controller = new ProfilesController(
            _profileServiceMock.Object,
            _jwtServiceMock.Object,
            _userSessionMock.Object,
            _loggerMock.Object
        );

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext(),
        };

        return controller;
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenSuccess_ReturnsOkWithResponse()
    {
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!",
        };

        _profileServiceMock
            .Setup(x => x.ChangePasswordAsync(request.CurrentPassword, request.NewPassword, request.ConfirmPassword))
            .ReturnsAsync(Result<(int, string)>.Success((123456, "jwt-token")));

        var controller = CreateController();

        var result = await controller.ChangePasswordAsync(request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ChangePasswordResponse>(ok.Value);
        Assert.Equal(123456, response.RecoveryCode);
        Assert.Equal("jwt-token", response.Token);
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenNewPasswordSameAsCurrent_ReturnsConflict()
    {
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "SamePassword123!",
            NewPassword = "SamePassword123!",
            ConfirmPassword = "SamePassword123!",
        };

        _profileServiceMock
            .Setup(x => x.ChangePasswordAsync(request.CurrentPassword, request.NewPassword, request.ConfirmPassword))
            .ReturnsAsync(Result<(int, string)>.Failure(ProfileErrors.NewPasswordSameAsCurrent));

        var controller = CreateController();

        var result = await controller.ChangePasswordAsync(request);

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status409Conflict, objectResult.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(ProfileProblemDetails.ChangePasswordConflict, problem.Title);
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenPasswordsDontMatch_ReturnsConflict()
    {
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "DifferentPassword123!",
        };

        _profileServiceMock
            .Setup(x => x.ChangePasswordAsync(request.CurrentPassword, request.NewPassword, request.ConfirmPassword))
            .ReturnsAsync(Result<(int, string)>.Failure(ProfileErrors.PasswordsDontMatch));

        var controller = CreateController();

        var result = await controller.ChangePasswordAsync(request);

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status409Conflict, objectResult.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(ProfileProblemDetails.ChangePasswordMismatch, problem.Title);
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenUserNotFound_ReturnsNotFound()
    {
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!",
        };

        _profileServiceMock
            .Setup(x => x.ChangePasswordAsync(request.CurrentPassword, request.NewPassword, request.ConfirmPassword))
            .ReturnsAsync(Result<(int, string)>.Failure(CommonErrors.UserNotFound));

        var controller = CreateController();

        var result = await controller.ChangePasswordAsync(request);

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(ProfileProblemDetails.UserNotFound, problem.Title);
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenInvalidCurrentPassword_ReturnsBadRequest()
    {
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "WrongPassword123!",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!",
        };

        _profileServiceMock
            .Setup(x => x.ChangePasswordAsync(request.CurrentPassword, request.NewPassword, request.ConfirmPassword))
            .ReturnsAsync(Result<(int, string)>.Failure(ProfileErrors.InvalidCurrentPassword));

        var controller = CreateController();

        var result = await controller.ChangePasswordAsync(request);

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(ProfileProblemDetails.InvalidCurrentPassword, problem.Title);
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenUnexpectedError_ReturnsInternalServerError()
    {
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!",
        };

        _profileServiceMock
            .Setup(x => x.ChangePasswordAsync(request.CurrentPassword, request.NewPassword, request.ConfirmPassword))
            .ReturnsAsync(Result<(int, string)>.Failure(CommonErrors.UnexpectedError));

        var controller = CreateController();

        var result = await controller.ChangePasswordAsync(request);

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(ProfileProblemDetails.UnexpectedError, problem.Title);
    }

    [Fact]
    public async Task ChangeUsernameAsync_WhenSuccess_ReturnsOkWithResponse()
    {
        var request = new ChangeUsernameRequest
        {
            CurrentUsername = "old-username",
            NewUsername = "new-username",
        };

        _profileServiceMock
            .Setup(x => x.ChangeUsernameAsync(request.CurrentUsername, request.NewUsername))
            .ReturnsAsync(Result<string>.Success("new-jwt-token"));

        var controller = CreateController();

        var result = await controller.ChangeUsernameAsync(request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ChangeUsernameResponse>(ok.Value);
        Assert.Equal("new-jwt-token", response.AccessToken);
    }

    [Fact]
    public async Task ChangeUsernameAsync_WhenNewUsernameSameAsCurrent_ReturnsConflict()
    {
        var request = new ChangeUsernameRequest
        {
            CurrentUsername = "same-username",
            NewUsername = "same-username",
        };

        _profileServiceMock
            .Setup(x => x.ChangeUsernameAsync(request.CurrentUsername, request.NewUsername))
            .ReturnsAsync(Result<string>.Failure(ProfileErrors.NewUsernameSameAsCurrent));

        var controller = CreateController();

        var result = await controller.ChangeUsernameAsync(request);

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status409Conflict, objectResult.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(ProfileProblemDetails.ChangeUsernameConflict, problem.Title);
    }

    [Fact]
    public async Task ChangeUsernameAsync_WhenUsernameAlreadyTaken_ReturnsConflict()
    {
        var request = new ChangeUsernameRequest
        {
            CurrentUsername = "old-username",
            NewUsername = "taken-username",
        };

        _profileServiceMock
            .Setup(x => x.ChangeUsernameAsync(request.CurrentUsername, request.NewUsername))
            .ReturnsAsync(Result<string>.Failure(ProfileErrors.UsernameAlreadyTaken));

        var controller = CreateController();

        var result = await controller.ChangeUsernameAsync(request);

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status409Conflict, objectResult.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(ProfileProblemDetails.ChangeUsernameConflict, problem.Title);
    }

    [Fact]
    public async Task ChangeUsernameAsync_WhenUserNotFound_ReturnsNotFound()
    {
        var request = new ChangeUsernameRequest
        {
            CurrentUsername = "old-username",
            NewUsername = "new-username",
        };

        _profileServiceMock
            .Setup(x => x.ChangeUsernameAsync(request.CurrentUsername, request.NewUsername))
            .ReturnsAsync(Result<string>.Failure(CommonErrors.UserNotFound));

        var controller = CreateController();

        var result = await controller.ChangeUsernameAsync(request);

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(ProfileProblemDetails.UserNotFound, problem.Title);
    }

    [Fact]
    public async Task ChangeUsernameAsync_WhenUnexpectedError_ReturnsInternalServerError()
    {
        var request = new ChangeUsernameRequest
        {
            CurrentUsername = "old-username",
            NewUsername = "new-username",
        };

        _profileServiceMock
            .Setup(x => x.ChangeUsernameAsync(request.CurrentUsername, request.NewUsername))
            .ReturnsAsync(Result<string>.Failure(CommonErrors.UnexpectedError));

        var controller = CreateController();

        var result = await controller.ChangeUsernameAsync(request);

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(ProfileProblemDetails.UnexpectedError, problem.Title);
    }

    [Fact]
    public async Task GetAsync_WhenCalled_ReturnsOkWithProfileData()
    {
        var controller = CreateController();

        var result = await controller.GetAsync();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<GetProfileResponse>(ok.Value);
        Assert.Equal(42, response.Id);
        Assert.Equal("test-login", response.Login);
        Assert.Equal("test-username", response.Username);
    }
}