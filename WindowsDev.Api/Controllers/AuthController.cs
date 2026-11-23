using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WindowsDev.Api.Logging;
using WindowsDev.Api.ProblemDetails;
using WindowsDev.Api.Urls;
using WindowsDev.Application.Identity.Authentication;
using WindowsDev.Application.Identity.Registration;
using WindowsDev.Contracts.Api.Identity;
using WindowsDev.Domain.Entities;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;
using WindowsDev.Domain.Messages.DialogsMessages.Warnings;

namespace WindowsDev.Api.Controllers;

[AllowAnonymous]
[Route("api/[controller]")]
[ApiController]
public class AuthController : ApiControllerBase
{
    private readonly IAuthentication _authentication;
    private readonly IRegistration _registration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthentication authentication,
        IRegistration registration,
        ILogger<AuthController> logger
    )
    {
        _authentication = authentication;
        _registration = registration;
        _logger = logger;
    }

    [HttpPost(AuthUrls.Register)]
    public async Task<ActionResult<UserRegisterResponse>> RegisterAsync(UserRegisterRequest request)
    {
        var result = await _registration.Register(
            request.Password,
            request.Login,
            request.Username
        );

        if (result.IsFailure)
        {
            switch (result.Error)
            {
                case AuthDialogWarnings.LoginTaken:
                    AuthControllerLogs.LoginAlreadyExist(_logger, request.Login);
                    return CustomProblem(
                        StatusCodes.Status409Conflict,
                        AuthProblemDetails.LoginConflict,
                        AuthProblemDetails.LoginConflictDetail(request.Login),
                        AuthDialogWarnings.LoginTaken
                    );

                case AuthDialogWarnings.UsernameTaken:
                    AuthControllerLogs.UsernameAlreadyExist(_logger, request.Username);
                    return CustomProblem(
                        StatusCodes.Status409Conflict,
                        AuthProblemDetails.UsernameConflict,
                        AuthProblemDetails.UsernameConflictDetail(request.Username),
                        AuthDialogWarnings.UsernameTaken
                    );

                default:
                    AuthControllerLogs.RegistrationError(_logger, request.Login);
                    return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        (int RecoveryCode, string JwtToken, UserRefreshToken RefreshToken) = result.Value;

        var response = new UserRegisterResponse(RecoveryCode, JwtToken, RefreshToken);
        AuthControllerLogs.RegistrationSuccess(_logger, request.Login);
        return Ok(response);
    }

    [HttpPost(AuthUrls.Login)]
    public async Task<ActionResult<UserLoginResponse>> LoginAsync(UserLoginRequest request)
    {
        var result = await _authentication.Authenticate(request.Login, request.Password);

        if (result.IsFailure)
        {
            AuthControllerLogs.LoginFailed(_logger, request.Login);

            switch (result.Error)
            {
                case AuthErrors.InvalidCredentials:
                    return CustomProblem(
                        StatusCodes.Status401Unauthorized,
                        AuthProblemDetails.InvalidCredentials,
                        AuthProblemDetails.InvalidCredentialsDetail,
                        result.Error
                    );

                default:
                    return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        var response = new UserLoginResponse
        {
            AccessToken = result.Value.Item1,
            RefreshToken = result.Value.Item2,
        };

        AuthControllerLogs.LoginSuccess(_logger, request.Login);
        return Ok(response);
    }

    [HttpPost(AuthUrls.RefreshToken)]
    public async Task<ActionResult<UserLoginResponse>> RefreshAsync(RefreshTokenRequest request)
    {
        var result = await _authentication.RefreshAsync(request.RefreshToken);

        if (result.IsFailure)
        {
            AuthControllerLogs.RefreshFailed(_logger);
            return CustomProblem(
                StatusCodes.Status401Unauthorized,
                AuthProblemDetails.RefreshTokenInvalid,
                AuthProblemDetails.RefreshTokenInvalidDetail,
                result.Error
                );
        }

        AuthControllerLogs.RefreshSuccess(_logger, request.RefreshToken.Id);
        return Ok(
            new UserLoginResponse
            {
                AccessToken = result.Value.Item1,
                RefreshToken = result.Value.Item2,
            }
        );
    }

    [HttpPost(AuthUrls.IsUsernameAvailable)]
    public async Task<ActionResult<IsUsernameAvailableResponse>> IsUsernameAvailableAsync(
        IsUsernameAvailableRequest request
    ) =>
        Ok(
            new IsUsernameAvailableResponse(
                await _registration.IsUsernameAvailableAsync(request.Username)
            )
        );

    [HttpPost(AuthUrls.IsLoginAvailable)]
    public async Task<ActionResult<IsLoginAvailableResponse>> IsLoginAvailableAsync(
        IsLoginAvailableRequest request
    ) =>
        Ok(
            new IsLoginAvailableResponse(
                await _registration.IsLoginAvailableAsync(request.Login)
            )
        );
}
