using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WindowsDev.Api.DTO.Request.AuthController;
using WindowsDev.Api.DTO.Response.AuthController;
using WindowsDev.Api.Logging;
using WindowsDev.Application.Identity;
using WindowsDev.Application.Identity.Authentication;
using WindowsDev.Application.Identity.Registration;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;
using WindowsDev.Domain.Messages.DialogsMessages.Warnings;

namespace WindowsDev.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
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

    [AllowAnonymous]
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
                    return Conflict();

                case AuthDialogWarnings.UsernameTaken:
                    AuthControllerLogs.UsernameAlreadyExist(_logger, request.Username);
                    return Conflict();

                default:
                    AuthControllerLogs.RegistrationError(_logger, request.Login);
                    return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        var response = new UserRegisterResponse { RecoveryCode = result.Value };
        AuthControllerLogs.RegistrationSuccess(_logger, request.Login);
        return Created(string.Empty, response);
    }

    [AllowAnonymous]
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
                    return Unauthorized();

                default:
                    return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        var response = new UserLoginResponse { JwtToken = result.Value };

        AuthControllerLogs.LoginSuccess(_logger, request.Login);
        return Ok(response);
    }

    //[AllowAnonymous]
    //[HttpPost("refresh")]
    //public async Task<ActionResult<RefreshTokensResponse>> RefreshTokens(int userId)
    //{
    //    var user = 
    //}
}
