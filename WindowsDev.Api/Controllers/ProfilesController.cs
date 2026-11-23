using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WindowsDev.Api.Logging;
using WindowsDev.Api.ProblemDetails;
using WindowsDev.Api.Urls;
using WindowsDev.Application.Identity;
using WindowsDev.Application.Identity.Cryptography;
using WindowsDev.Application.Users;
using WindowsDev.Contracts.Api.Profiles;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;

namespace WindowsDev.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProfilesController : ApiControllerBase
{
    private readonly ILogger<ProfilesController> _logger;
    private readonly IProfileService _profileService;
    private readonly IJwtService _jwtService;
    private readonly IUserSession _userSession;

    public ProfilesController(
        IProfileService profileService,
        IJwtService jwtService,
        IUserSession userSession,
        ILogger<ProfilesController> logger
    )
    {
        _profileService = profileService;
        _jwtService = jwtService;
        _userSession = userSession;
        _logger = logger;
    }

    [Authorize]
    [HttpPost(ProfileUrls.ChangePassword)]
    public async Task<ActionResult<ChangePasswordResponse>> ChangePasswordAsync(
        ChangePasswordRequest request
    )
    {
        var result = await _profileService.ChangePasswordAsync(
            request.CurrentPassword,
            request.NewPassword,
            request.ConfirmPassword
        );

        if (result.IsFailure)
        {
            ProfilesControllerLogs.ChangePasswordFailed(_logger, _userSession.Login);
            return result.Error switch
            {
                ProfileErrors.NewPasswordSameAsCurrent => CustomProblem(
                    StatusCodes.Status409Conflict,
                    ProfileProblemDetails.ChangePasswordConflict,
                    ProfileProblemDetails.ChangePasswordConflictDetail(_userSession.Login),
                    result.Error
                ),

                ProfileErrors.PasswordsDontMatch => CustomProblem(
                    StatusCodes.Status409Conflict,
                    ProfileProblemDetails.ChangePasswordMismatch,
                    ProfileProblemDetails.ChangePasswordMismatchDetail(_userSession.Login),
                    result.Error
                ),

                CommonErrors.UserNotFound => CustomProblem(
                    StatusCodes.Status404NotFound,
                    ProfileProblemDetails.UserNotFound,
                    ProfileProblemDetails.UserNotFoundDetail(_userSession.Login),
                    result.Error
                ),

                ProfileErrors.InvalidCurrentPassword => CustomProblem(
                    StatusCodes.Status400BadRequest,
                    ProfileProblemDetails.InvalidCurrentPassword,
                    ProfileProblemDetails.InvalidCurrentPasswordDetail(_userSession.Login),
                    result.Error
                ),

                _ => CustomProblem(
                    StatusCodes.Status500InternalServerError,
                    ProfileProblemDetails.UnexpectedError,
                    ProfileProblemDetails.UnexpectedErrorDetail(_userSession.Login),
                    result.Error
                ),
            };
        }

        var jwtToken = result.Value.Item2;
        var recoveryCode = result.Value.Item1;
        var response = new ChangePasswordResponse
        {
            Token = jwtToken,
            RecoveryCode = recoveryCode,
        };

        ProfilesControllerLogs.ChangePasswordSuccessful(_logger, _userSession.Login);
        return Ok(response);
    }

    [Authorize]
    [HttpPost(ProfileUrls.ChangeUsername)]
    public async Task<ActionResult<ChangeUsernameResponse>> ChangeUsernameAsync(
        ChangeUsernameRequest request
    )
    {
        var result = await _profileService.ChangeUsernameAsync(
            request.CurrentUsername,
            request.NewUsername
        );

        if (result.IsFailure)
        {
            ProfilesControllerLogs.ChangeUsernameFailed(_logger, _userSession.Login);
            return result.Error switch
            {
                ProfileErrors.NewUsernameSameAsCurrent => CustomProblem(
                    StatusCodes.Status409Conflict,
                    ProfileProblemDetails.ChangeUsernameConflict,
                    ProfileProblemDetails.ChangeUsernameSameAsCurrentDetail(_userSession.Login),
                    result.Error
                ),

                ProfileErrors.UsernameAlreadyTaken => CustomProblem(
                    StatusCodes.Status409Conflict,
                    ProfileProblemDetails.ChangeUsernameConflict,
                    ProfileProblemDetails.ChangeUsernameConflictDetail(_userSession.Login),
                    result.Error
                ),

                CommonErrors.UserNotFound => CustomProblem(
                    StatusCodes.Status404NotFound,
                    ProfileProblemDetails.UserNotFound,
                    ProfileProblemDetails.UserNotFoundDetail(_userSession.Login),
                    result.Error
                ),

                _ => CustomProblem(
                    StatusCodes.Status500InternalServerError,
                    ProfileProblemDetails.UnexpectedError,
                    ProfileProblemDetails.UnexpectedErrorDetail(_userSession.Login),
                    result.Error
                ),
            };
        }

        var response = new ChangeUsernameResponse { AccessToken = result.Value };

        ProfilesControllerLogs.ChangeUsernameSuccessful(_logger, _userSession.Login);
        return Ok(response);
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<GetProfileResponse>> GetAsync()
    {
        var response = new GetProfileResponse
        {
            Id = _userSession.UserId,
            Login = _userSession.Login,
            Username = _userSession.Username,
        };

        ProfilesControllerLogs.GetProfileSuccessful(_logger, _userSession.UserId);
        return Ok(response);
    }
}
