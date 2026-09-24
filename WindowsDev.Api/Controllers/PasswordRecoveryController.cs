using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WindowsDev.Api.DTO.Request;
using WindowsDev.Api.DTO.Response;
using WindowsDev.Api.Logging;
using WindowsDev.Application.Identity.PasswordRecovery;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;

namespace WindowsDev.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PasswordRecoveryController : ControllerBase
{
    private readonly ILogger<PasswordRecoveryController> _logger;
    private readonly IPasswordRecoveryService _passwordRecoveryService;

    public PasswordRecoveryController(IPasswordRecoveryService passwordRecoveryService, ILogger<PasswordRecoveryController> logger)
    {
        _passwordRecoveryService = passwordRecoveryService;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpPost("verifyRecoveryCode")]
    public async Task<ActionResult<VerifyPasswordRecoveryCodeResponse>> IsRecoveryCodeCorrectAsync(VerifyPasswordRecoveryCodeRequest request)
    {
        var result = await _passwordRecoveryService.IsRecoverCodeCorrectAsync(request.recoveryCode, request.login);

        if (result.IsFailure)
        {
            switch (result.Error)
            {
                case PasswordRecoveryErrors.InvalidRecoveryCode:
                    PasswordRecoveryControllerLogs.InvalidRecoveryCode(_logger, request.login);
                    return UnprocessableEntity();

                default:
                    PasswordRecoveryControllerLogs.UnexpectedError(_logger, request.login);
                    return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        PasswordRecoveryControllerLogs.RecoveryCodeIsValid(_logger, request.login);
        var response = new VerifyPasswordRecoveryCodeResponse { IsValid = result.Value };

        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("recoveryPassword")]
    public async Task<ActionResult<ChangePasswordResponse>> ChangePasswordAsync(ChangePasswordRequest request)
    {
        var result = await _passwordRecoveryService.ChangePasswordAsync(request.Login, request.Password);

        if (result.IsFailure)
        {
            switch (result.Error)
            {
                case CommonErrors.UserNotFound:
                    CommonLogs.UserNotFound(_logger, request.Login);
                    return BadRequest();
                default:
                    PasswordRecoveryControllerLogs.UnexpectedError(_logger, request.Login);
                    return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        PasswordRecoveryControllerLogs.PasswordRecoverySuccesful(_logger, request.Login);
        var response = new ChangePasswordResponse { RecoveryCode = result.Value };
        return Ok(response);

    }

    [AllowAnonymous]
    [HttpPost("isUserExist")]
    public async Task<ActionResult<IsUserExistResponse>> IsUserExist(IsUserExistRequest request)
    {
        var result = await _passwordRecoveryService.IsUserExistAsync(request.Login);
        var response = new IsUserExistResponse { IsExist = result.Value };

        return Ok(response);
    }
}