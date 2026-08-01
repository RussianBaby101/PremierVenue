using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using PremierVenue.Core.DTOs;
using PremierVenue.Core.Services;

namespace PremierVenue.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
// Manages user accounts, roles, and staff invitations for administrators
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }


    // Get all users, optionally filtered by role

    [HttpGet]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<ResponseDto<List<UserDto>>>> GetUsers([FromQuery] string? role = null)
    {
        try
        {
            var users = await _userService.GetAllUsersAsync();
            if (!string.IsNullOrWhiteSpace(role))
                users = users.Where(u => u.Role.Equals(role, StringComparison.OrdinalIgnoreCase)).ToList();

            return Ok(ResponseDto<List<UserDto>>.SuccessResponse(users, "Users retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return StatusCode(500, ResponseDto<List<UserDto>>.ErrorResponse("An error occurred while retrieving users"));
        }
    }


    // Get a specific user by ID

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<ResponseDto<UserDto>>> GetUser(int id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(ResponseDto<UserDto>.ErrorResponse("User not found"));

            return Ok(ResponseDto<UserDto>.SuccessResponse(user, "User retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with ID {UserId}", id);
            return StatusCode(500, ResponseDto<UserDto>.ErrorResponse("An error occurred while retrieving the user"));
        }
    }

    [HttpGet("me")]
    public async Task<ActionResult<ResponseDto<UserDto>>> GetMyProfile()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(claim, out var userId))
            return Unauthorized(ResponseDto<UserDto>.ErrorResponse("User is not authenticated"));

        var user = await _userService.GetUserByIdAsync(userId);
        return user == null
            ? NotFound(ResponseDto<UserDto>.ErrorResponse("User not found"))
            : Ok(ResponseDto<UserDto>.SuccessResponse(user, "Profile retrieved successfully"));
    }

    [HttpPut("me")]
    public async Task<ActionResult<ResponseDto<UserDto>>> UpdateMyProfile([FromBody] UpdateUserDto model)
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(claim, out var userId))
            return Unauthorized(ResponseDto<UserDto>.ErrorResponse("User is not authenticated"));

        var user = await _userService.UpdateUserAsync(userId, model);
        return user == null
            ? NotFound(ResponseDto<UserDto>.ErrorResponse("User not found"))
            : Ok(ResponseDto<UserDto>.SuccessResponse(user, "Profile updated successfully"));
    }


    // Toggle a user's active status (activate/deactivate)

    [HttpPatch("{id}/toggle-status")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<ResponseDto<UserDto>>> ToggleUserStatus(int id)
    {
        try
        {
            var user = await _userService.ToggleUserStatusAsync(id);
            if (user == null)
                return NotFound(ResponseDto<UserDto>.ErrorResponse("User not found"));

            return Ok(ResponseDto<UserDto>.SuccessResponse(user, "User status updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling status for user with ID {UserId}", id);
            return StatusCode(500, ResponseDto<UserDto>.ErrorResponse("An error occurred while updating user status"));
        }
    }


    // Create a staff user and send an email invitation

    [HttpPost("invitations")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<ResponseDto<UserDto>>> CreateStaffInvitation([FromBody] CreateStaffInvitationDto model)
    {
        try
        {
            var user = await _userService.CreateStaffInvitationAsync(model);
            return Ok(ResponseDto<UserDto>.SuccessResponse(user, "Invitation created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ResponseDto<UserDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating staff invitation");
            return StatusCode(500, ResponseDto<UserDto>.ErrorResponse("An error occurred while sending the invitation"));
        }
    }
}
