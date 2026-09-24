using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WindowsDev.Api.DTO.Request.ProfilesController;
using WindowsDev.Api.DTO.Response.ProfilesController;
using WindowsDev.Api.Logging;
using WindowsDev.Application.Identity;
using WindowsDev.Application.Users;
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
    public async Task<ActionResult<ProfileChangePasswordResponse>> ChangePasswordAsync(
        ProfileChangePasswordRequest request
    )
    {
        var result = await _profileService.ChangePasswordAsync(
            request.CurrentPassword,
            request.NewPassword,
            request.ConfirmPassword
        );

        if (result.IsFailure)
        {
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
        var response = new ProfileChangePasswordResponse
        {
            JwtToken = jwtToken,
            RecoveryCode = recoveryCode,
        };

        return Ok(response);
    }

    [Authorize]
    [HttpPost("changeUsername")]
    public async Task<ActionResult<UsernameChangeResponse>> ChangeUsernameAsync(
        UsernameChangeRequest request
    )
    {
        var result = await _profileService.ChangeUsernameAsync(
            request.CurrentUsername,
            request.NewUsername
        );

        if (result.IsFailure)
        {
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

        var response = new UsernameChangeResponse { AccessToken = result.Value };
        return Ok(response);
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<GetAsyncResponse>> GetAsync()
    {
        var response = new GetAsyncResponse
        {
            Id = _userSession.UserId,
            Login = _userSession.Login,
            Username = _userSession.Username,
        };

        return Ok(response);
    }
}
