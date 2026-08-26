using Microsoft.AspNetCore.Mvc;
using WindowsDev.Api.DTOs.Request;
using WindowsDev.Api.DTOs.Response;
using WindowsDev.Api.Logging;
using WindowsDev.Application.Services.Registration;
using WindowsDev.Domain.Messages.DialogsMessages.Warnings;
using WindowsDev.Infrastructure.Logging;

namespace WindowsDev.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IRegistration _registration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IRegistration regitration, ILogger<AuthController> logger)
    {
        _registration = regitration;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserRegisterResponse>> Register(UserRegisterRequest request)
    {
        var result = await _registration.Register(
            request.Password,
            request.Login,
            request.Username);

        if (!result.IsSuccess)
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
                    AuthControllerLogs.RegistrationFailed(_logger, request.Login);
                    return BadRequest();
            }
        }

        var response = new UserRegisterResponse { RecoveryCode = result.Value };
        AuthControllerLogs.RegistrationSucces(_logger, request.Login);
        return Created(string.Empty, response);
    }
    
    [HttpPost("login")]
        public async Task<ActionResult<string>> Login()
}
