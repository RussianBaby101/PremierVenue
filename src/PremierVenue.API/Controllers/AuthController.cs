using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PremierVenue.Core.DTOs;
using PremierVenue.Core.Services;

namespace PremierVenue.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
// Handles user authentication, registration, password reset, and token refresh flows
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, ILogger<AuthController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    // Register a new client account
    [HttpPost("register")]
    public async Task<ActionResult<ResponseDto<AuthResponseDto>>> Register([FromBody] CreateUserDto model)
    {
        try
        {
            var result = await _userService.RegisterAsync(model);
            return Ok(ResponseDto<AuthResponseDto>.SuccessResponse(result, "Registration successful"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ResponseDto<AuthResponseDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return StatusCode(500, ResponseDto<AuthResponseDto>.ErrorResponse("An error occurred during registration"));
        }
    }

    // Authenticate a user and return JWT tokens
    [HttpPost("login")]
    public async Task<ActionResult<ResponseDto<AuthResponseDto>>> Login([FromBody] LoginDto model)
    {
        try
        {
            var result = await _userService.LoginAsync(model);
            return Ok(ResponseDto<AuthResponseDto>.SuccessResponse(result, "Login successful"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ResponseDto<AuthResponseDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, ResponseDto<AuthResponseDto>.ErrorResponse("An error occurred during login"));
        }
    }

    // Request a password reset email
    [HttpPost("forgot-password")]
    public async Task<ActionResult<ResponseDto<string>>> ForgotPassword([FromBody] ForgotPasswordDto model)
    {
        try
        {
            await _userService.RequestPasswordResetAsync(model);
            return Ok(ResponseDto<string>.SuccessResponse(string.Empty, "Password reset instructions sent if the account is active."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ResponseDto<string>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting password reset");
            return StatusCode(500, ResponseDto<string>.ErrorResponse("An error occurred while requesting password reset"));
        }
    }

    // Complete an OTP password reset.
    [HttpPost("reset-password")]
    public async Task<ActionResult<ResponseDto<string>>> ResetPassword([FromBody] ResetPasswordDto model)
    {
        try
        {
            await _userService.ResetPasswordAsync(model);
            return Ok(ResponseDto<string>.SuccessResponse(string.Empty, "Password reset successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ResponseDto<string>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password");
            return StatusCode(500, ResponseDto<string>.ErrorResponse("An error occurred while resetting the password"));
        }
    }

    // Refresh access token using a refresh token
    [HttpPost("refresh")]
    public async Task<ActionResult<ResponseDto<AuthResponseDto>>> Refresh([FromBody] RefreshTokenDto model)
    {
        try
        {
            var result = await _userService.RefreshTokenAsync(model);
            return Ok(ResponseDto<AuthResponseDto>.SuccessResponse(result, "Token refreshed"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ResponseDto<AuthResponseDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return StatusCode(500, ResponseDto<AuthResponseDto>.ErrorResponse("An error occurred while refreshing token"));
        }
    }

    // Get invitation details by token
    [HttpGet("invitation")]
    public async Task<ActionResult<ResponseDto<InvitationDto>>> GetInvitation([FromQuery] string token)
    {
        try
        {
            var invitation = await _userService.GetInvitationByTokenAsync(token);
            if (invitation == null)
                return BadRequest(ResponseDto<InvitationDto>.ErrorResponse("Invalid or expired invitation token"));

            return Ok(ResponseDto<InvitationDto>.SuccessResponse(invitation, "Invitation retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invitation");
            return StatusCode(500, ResponseDto<InvitationDto>.ErrorResponse("An error occurred while retrieving the invitation"));
        }
    }

    // Accept a staff invitation and set the account password
    [HttpPost("accept-invitation")]
    public async Task<ActionResult<ResponseDto<UserDto>>> AcceptInvitation([FromBody] AcceptInvitationDto model)
    {
        try
        {
            var user = await _userService.AcceptInvitationAsync(model);
            if (user == null)
                return BadRequest(ResponseDto<UserDto>.ErrorResponse("Invalid or expired invitation token"));

            return Ok(ResponseDto<UserDto>.SuccessResponse(user, "Invitation accepted. Your account is now active."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ResponseDto<UserDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting invitation");
            return StatusCode(500, ResponseDto<UserDto>.ErrorResponse("An error occurred while accepting the invitation"));
        }
    }

    // Logout the current user
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return Ok(ResponseDto<string>.SuccessResponse(string.Empty, "Logout successful"));
    }
}
