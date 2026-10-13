using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WindowsDev.Api.Logging;
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
    [HttpPost("changePassword")]
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
                    "Password Conflict",
                    "The new password cannot be identical to the current one.",
                    result.Error
                ),

                ProfileErrors.PasswordsDontMatch => CustomProblem(
                    StatusCodes.Status409Conflict,
                    "Password Conflict",
                    "The password and confirmation password do not match.",
                    result.Error
                ),

                CommonErrors.UserNotFound => CustomProblem(
                    StatusCodes.Status404NotFound,
                    "User Not Found",
                    "The requested user profile was not found.",
                    result.Error
                ),

                ProfileErrors.InvalidCurrentPassword => CustomProblem(
                    StatusCodes.Status400BadRequest,
                    "Invalid Current Password",
                    "The current password you entered is incorrect.",
                    result.Error
                ),

                _ => throw new NotImplementedException(),
            };
        }

        var jwtToken = result.Value.Item2;
        var recoveryCode = result.Value.Item1;
        var response = new ChangePasswordResponse
        {
            JwtToken = jwtToken,
            RecoveryCode = recoveryCode,
        };

        ProfilesControllerLogs.ChangePasswordSuccessful(_logger, _userSession.Login);
        return Ok(response);
    }

    [Authorize]
    [HttpPost("changeUsername")]
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
                    "Username Conflict",
                    "The new username cannot be identical to the current one.",
                    result.Error
                ),

                ProfileErrors.UsernamAlreadyTaken => CustomProblem(
                    StatusCodes.Status409Conflict,
                    "Username Conflict",
                    "The new username is already taken.",
                    result.Error
                ),

                CommonErrors.UserNotFound => CustomProblem(
                    StatusCodes.Status404NotFound,
                    "User Not Found",
                    "The requested user profile was not found.",
                    result.Error
                ),

                _ => throw new NotImplementedException(),
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
