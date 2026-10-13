using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WindowsDev.Api.Logging;
using WindowsDev.Api.ProblemDetails;
using WindowsDev.Application.Identity.Authentication;
using WindowsDev.Application.Identity.Cryptography;
using WindowsDev.Application.Identity.Registration;
using WindowsDev.Contracts.Api.Identity;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;
using WindowsDev.Domain.Messages.DialogsMessages.Warnings;

namespace WindowsDev.Api.Controllers;

[AllowAnonymous]
[Route("api/[controller]")]
[ApiController]
public class AuthController : ApiControllerBase
{
    private readonly IJwtService _jwtService;
    private readonly IAuthentication _authentication;
    private readonly IRegistration _registration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthentication authentication,
        IRegistration regitration,
        ILogger<AuthController> logger,
        IJwtService jwtService
    )
    {
        _authentication = authentication;
        _registration = regitration;
        _logger = logger;
        _jwtService = jwtService;
    }

    [HttpPost("register")]
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
                        AuthDialogWarnings.LoginTaken);

                case AuthDialogWarnings.UsernameTaken:
                    AuthControllerLogs.UsernameAlreadyExist(_logger, request.Username);
                    return CustomProblem(
                        StatusCodes.Status409Conflict,
                        AuthProblemDetails.UsernameConflict,
                        AuthProblemDetails.UsernameConflictDetail(request.Username),
                        AuthDialogWarnings.UsernameTaken);

                default:
                    AuthControllerLogs.RegistrationError(_logger, request.Login);
                    return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        var response = new UserRegisterResponse { RecoveryCode = result.Value };
        AuthControllerLogs.RegistrationSuccess(_logger, request.Login);
        return Created(string.Empty, response);
    }

    [HttpPost("login")]
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
                        AuthErrors.InvalidCredentials);

                default:
                    return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        var response = new UserLoginResponse { JwtToken = result.Value };

        AuthControllerLogs.LoginSuccess(_logger, request.Login);
        return Ok(response);
    }
}
