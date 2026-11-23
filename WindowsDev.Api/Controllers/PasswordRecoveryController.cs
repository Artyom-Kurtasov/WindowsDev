using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WindowsDev.Api.Logging;
using WindowsDev.Api.ProblemDetails;
using WindowsDev.Api.Urls;
using WindowsDev.Application.Identity.PasswordRecovery;
using WindowsDev.Contracts.Api.PasswordRecovery;
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

    [HttpPost(PasswordRecoveryUrls.VerifyRecoveryCode)]
    public async Task<ActionResult<VerifyRecoveryCodeResponse>> IsRecoveryCodeCorrectAsync(
        [FromBody] VerifyRecoveryCodeRequest request
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
                        PasswordRecoveryProblemDetails.InvalidRecoveryCode,
                        PasswordRecoveryProblemDetails.InvalidRecoveryCodeDetail(request.login),
                        result.Error
                    );

                default:
                    PasswordRecoveryControllerLogs.UnexpectedError(_logger, request.login);
                    return CustomProblem(
                        StatusCodes.Status500InternalServerError,
                        PasswordRecoveryProblemDetails.UnexpectedError,
                        PasswordRecoveryProblemDetails.UnexpectedErrorDetail(request.login),
                        result.Error
                    );
            }
        }

        PasswordRecoveryControllerLogs.RecoveryCodeIsValid(_logger, request.login);
        var response = new VerifyRecoveryCodeResponse { IsValid = result.Value };

        return Ok(response);
    }

    [HttpPost(PasswordRecoveryUrls.ChangePassword)]
    public async Task<ActionResult<ResetPasswordResponse>> ChangePasswordAsync(
        [FromBody] ResetPasswordRequest request
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
                        PasswordRecoveryProblemDetails.UserNotFound,
                        PasswordRecoveryProblemDetails.UserNotFoundDetail(request.Login),
                        result.Error
                    );
                default:
                    PasswordRecoveryControllerLogs.UnexpectedError(_logger, request.Login);
                    return CustomProblem(
                        StatusCodes.Status500InternalServerError,
                        PasswordRecoveryProblemDetails.PasswordChangeFailed,
                        PasswordRecoveryProblemDetails.PasswordChangeFailedDetail(request.Login),
                        result.Error
                    );
            }
        }

        PasswordRecoveryControllerLogs.PasswordRecoverySuccesful(_logger, request.Login);
        var response = new ResetPasswordResponse { RecoveryCode = result.Value.Item1 };
        return Ok(response);
    }

    [HttpPost(PasswordRecoveryUrls.CheckUser)]
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
