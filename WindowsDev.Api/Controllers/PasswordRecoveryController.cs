using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WindowsDev.Api.DTO.Request;
using WindowsDev.Api.DTO.Response;
using WindowsDev.Api.Logging;
using WindowsDev.Application.Identity.PasswordRecovery;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;

namespace WindowsDev.Api.Controllers;

[AllowAnonymous]
[Route("api/[controller]")]
[ApiController]
public class PasswordRecoveryController : ApiControllerBase
{
    private readonly ILogger<PasswordRecoveryController> _logger;
    private readonly IPasswordRecoveryService _passwordRecoveryService;

    public PasswordRecoveryController(
        IPasswordRecoveryService passwordRecoveryService,
        ILogger<PasswordRecoveryController> logger
    )
    {
        _passwordRecoveryService = passwordRecoveryService;
        _logger = logger;
    }

    [HttpPost("verify-code")]
    public async Task<ActionResult<VerifyPasswordRecoveryCodeResponse>> IsRecoveryCodeCorrectAsync(
        [FromBody] VerifyPasswordRecoveryCodeRequest request
    )
    {
        var result = await _passwordRecoveryService.IsRecoverCodeCorrectAsync(
            request.recoveryCode,
            request.login
        );

        if (result.IsFailure)
        {
            switch (result.Error)
            {
                case PasswordRecoveryErrors.InvalidRecoveryCode:
                    PasswordRecoveryControllerLogs.InvalidRecoveryCode(_logger, request.login);
                    return CustomProblem(
                        StatusCodes.Status422UnprocessableEntity,
                        "Invalid Recovery Code",
                        "The recovery code provided is invalid or has expired.",
                        result.Error
                    );

                default:
                    PasswordRecoveryControllerLogs.UnexpectedError(_logger, request.login);
                    return CustomProblem(
                        StatusCodes.Status500InternalServerError,
                        "Unexpected Error",
                        "An unexpected error occurred during code verification.",
                        result.Error
                    );
            }
        }

        PasswordRecoveryControllerLogs.RecoveryCodeIsValid(_logger, request.login);
        var response = new VerifyPasswordRecoveryCodeResponse { IsValid = result.Value };

        return Ok(response);
    }

    [HttpPost("change-password")]
    public async Task<ActionResult<ChangePasswordResponse>> ChangePasswordAsync(
        [FromBody] ChangePasswordRequest request
    )
    {
        var result = await _passwordRecoveryService.ChangePasswordAsync(
            request.Login,
            request.Password
        );

        if (result.IsFailure)
        {
            switch (result.Error)
            {
                case CommonErrors.UserNotFound:
                    CommonLogs.UserNotFound(_logger, request.Login);
                    return CustomProblem(
                        StatusCodes.Status400BadRequest,
                        "User Not Found",
                        $"User with login '{request.Login}' was not found.",
                        result.Error
                    );
                default:
                    PasswordRecoveryControllerLogs.UnexpectedError(_logger, request.Login);
                    return CustomProblem(
                        StatusCodes.Status500InternalServerError,
                        "Unexpected Error",
                        "An unexpected error occurred during password changes.",
                        result.Error
                    );
            }
        }

        PasswordRecoveryControllerLogs.PasswordRecoverySuccesful(_logger, request.Login);
        var response = new ChangePasswordResponse { RecoveryCode = result.Value.Item1 };
        return Ok(response);
    }

    [HttpPost("check-user")]
    public async Task<ActionResult<IsUserExistResponse>> IsUserExist(
        [FromBody] IsUserExistRequest request
    )
    {
        var result = await _passwordRecoveryService.IsUserExistAsync(request.Login);
        var response = new IsUserExistResponse { IsExist = result.Value };

        PasswordRecoveryControllerLogs.CheckUserSuccessful(_logger, request.Login, result.Value);
        return Ok(response);
    }
}
