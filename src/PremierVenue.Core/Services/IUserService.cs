using PremierVenue.Core.DTOs;
namespace PremierVenue.Core.Services;

public interface IUserService
{
    Task<AuthResponseDto> RegisterAsync(CreateUserDto model);
    Task<AuthResponseDto> LoginAsync(LoginDto model);
    Task RequestPasswordResetAsync(ForgotPasswordDto model);
    Task ResetPasswordAsync(ResetPasswordDto model);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto model);
    Task<UserDto?> GetUserByIdAsync(int id);
    Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto model);
    Task<bool> DeleteUserAsync(int id);
    Task<List<UserDto>> GetAllUsersAsync();
    Task<UserDto?> ToggleUserStatusAsync(int id);
    Task<UserDto> CreateStaffInvitationAsync(CreateStaffInvitationDto model);
    Task<InvitationDto?> GetInvitationByTokenAsync(string token);
    Task<UserDto?> AcceptInvitationAsync(AcceptInvitationDto model);
}