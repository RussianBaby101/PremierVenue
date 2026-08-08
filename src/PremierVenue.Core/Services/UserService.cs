using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using PremierVenue.Core.DTOs;
using PremierVenue.Core.Utilities;
using PremierVenue.Domain.Entities;
using PremierVenue.Domain.Enums;
using PremierVenue.Domain.Interfaces;

namespace PremierVenue.Core.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;

    public UserService(IUserRepository userRepository, IUnitOfWork unitOfWork, IConfiguration configuration, IEmailService emailService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _emailService = emailService;
    }

    public async Task<AuthResponseDto> RegisterAsync(CreateUserDto model)
    {
        var existing = await _userRepository.GetByEmailAsync(model.Email);
        if (existing != null)
            throw new InvalidOperationException("A user with this email already exists");

        var user = new User
        {
            Email = model.Email,
            PasswordHash = PasswordHasher.HashPassword(model.Password),
            FirstName = model.FirstName,
            LastName = model.LastName,
            PhoneNumber = model.PhoneNumber,
            Role = UserRole.Client,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto model)
    {
        var user = await _userRepository.GetByEmailAsync(model.Email);
        if (user == null || !user.IsActive || !PasswordHasher.VerifyPassword(model.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Incorrect email or password");

        user.LastLoginAt = DateTime.UtcNow;
        return await GenerateAuthResponseAsync(user);
    }

    public async Task RequestPasswordResetAsync(ForgotPasswordDto model)
    {
        var user = await _userRepository.GetByEmailAsync(model.Email.Trim());
        if (user == null || !user.IsActive)
            return;

        var otp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        var resetToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var now = DateTime.UtcNow;
        user.InvitationToken = $"password-reset:{HashResetCode(resetToken)}:{HashResetCode(otp)}";
        user.InvitationSentAt = now;
        user.InvitationExpiresAt = now.AddMinutes(10);
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();

        var baseUrl = _configuration["App:FrontendBaseUrl"]?.TrimEnd('/') ?? "https://localhost:5002";
        var resetLink = $"{baseUrl}/pages/public/reset-password.html?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(resetToken)}";

        await _emailService.SendEmailAsync(
            user.Email,
            "PremierVenue password reset code",
            BuildPasswordResetEmailBody(user.FirstName, otp, resetLink));
    }

    public async Task ResetPasswordAsync(ResetPasswordDto model)
    {
        var passwordErrors = PasswordPolicy.GetValidationErrors(model.Password);
        if (passwordErrors.Count > 0)
            throw new InvalidOperationException(string.Join(" ", passwordErrors));

        var user = await _userRepository.GetByEmailAsync(model.Email.Trim());
        var expectedPrefix = "password-reset:";
        var storedToken = user?.InvitationToken;
        var storedParts = storedToken?.StartsWith(expectedPrefix, StringComparison.Ordinal) == true
            ? storedToken[expectedPrefix.Length..].Split(':', 2)
            : Array.Empty<string>();
        var storedTokenHash = storedParts.Length == 2 ? storedParts[0] : null;
        var storedOtpHash = storedParts.Length == 2 ? storedParts[1] : null;
        var suppliedTokenHash = HashResetCode(model.Token.Trim());
        var suppliedOtpHash = HashResetCode(model.Otp.Trim());

        if (user == null || !user.IsActive || user.InvitationExpiresAt <= DateTime.UtcNow ||
            string.IsNullOrWhiteSpace(model.Token) || string.IsNullOrWhiteSpace(storedTokenHash) ||
            string.IsNullOrWhiteSpace(storedOtpHash) ||
            !CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(storedTokenHash),
                Encoding.UTF8.GetBytes(suppliedTokenHash)) ||
            !CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(storedOtpHash),
                Encoding.UTF8.GetBytes(suppliedOtpHash)))
        {
            throw new InvalidOperationException("The reset link or code is invalid or has expired.");
        }

        user.PasswordHash = PasswordHasher.HashPassword(model.Password);
        user.InvitationToken = null;
        user.InvitationSentAt = null;
        user.InvitationExpiresAt = null;
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();
    }

    private static string HashResetCode(string code)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(code)));
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto model)
    {
        var user = await _userRepository.GetByRefreshTokenAsync(model.RefreshToken);
        if (user == null)
            throw new UnauthorizedAccessException("Invalid refresh token");

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user == null ? null : MapToUserDto(user);
    }

    public async Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto model)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            return null;

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.PhoneNumber = model.PhoneNumber;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return MapToUserDto(user);
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            return false;

        user.IsActive = false;
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(MapToUserDto).ToList();
    }

    public async Task<UserDto?> ToggleUserStatusAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            return null;

        user.IsActive = !user.IsActive;
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();
        return MapToUserDto(user);
    }

    public async Task<UserDto> CreateStaffInvitationAsync(CreateStaffInvitationDto model)
    {
        var existing = await _userRepository.GetByEmailAsync(model.Email);
        if (existing != null)
            throw new InvalidOperationException("A user with this email address already exists");

        var (firstName, lastName) = SplitFullName(model.FullName);
        var userName = model.Email.Split('@')[0];
        var token = Guid.NewGuid().ToString("N");

        var user = new User
        {
            Email = model.Email,
            UserName = userName,
            PasswordHash = string.Empty,
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = string.Empty,
            Role = UserRole.Staff,
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
            InvitationToken = token,
            InvitationSentAt = DateTime.UtcNow,
            InvitationExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var baseUrl = _configuration["App:FrontendBaseUrl"]?.TrimEnd('/') ?? "https://localhost:5002";
        var link = $"{baseUrl}/pages/accept-invitation.html?token={token}";

        try
        {
            await _emailService.SendEmailAsync(
                user.Email,
                "PremierVenue Staff Invitation",
                BuildStaffInvitationEmailBody(firstName, userName, link));
        }
        catch
        {
            _userRepository.Delete(user);
            await _unitOfWork.SaveChangesAsync();
            throw;
        }

        return MapToUserDto(user);
    }

    public async Task<InvitationDto?> GetInvitationByTokenAsync(string token)
    {
        var user = await _userRepository.GetByInvitationTokenAsync(token);
        if (user == null || user.InvitationExpiresAt < DateTime.UtcNow)
            return null;

        return new InvitationDto
        {
            Email = user.Email,
            UserName = user.UserName,
            FullName = $"{user.FirstName} {user.LastName}",
            ExpiresAt = user.InvitationExpiresAt
        };
    }

    public async Task<UserDto?> AcceptInvitationAsync(AcceptInvitationDto model)
    {
        var user = await _userRepository.GetByInvitationTokenAsync(model.Token);
        if (user == null || user.InvitationExpiresAt < DateTime.UtcNow)
            throw new InvalidOperationException("Invalid or expired invitation token");

        var passwordErrors = PasswordPolicy.GetValidationErrors(model.Password);
        if (passwordErrors.Count > 0)
            throw new InvalidOperationException(string.Join(" ", passwordErrors));

        user.PasswordHash = PasswordHasher.HashPassword(model.Password);
        user.IsActive = true;
        user.InvitationToken = null;
        user.InvitationSentAt = null;
        user.InvitationExpiresAt = null;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return MapToUserDto(user);
    }

    private static (string FirstName, string LastName) SplitFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return (string.Empty, string.Empty);

        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var firstName = parts[0];
        var lastName = parts.Length > 1 ? string.Join(' ', parts.Skip(1)) : string.Empty;
        return (firstName, lastName);
    }

    private static string BuildPasswordResetEmailBody(string firstName, string otp, string resetLink)
    {
        var safeFirstName = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(firstName) ? "there" : firstName);
        var safeOtp = WebUtility.HtmlEncode(otp);
        var safeResetLink = WebUtility.HtmlEncode(resetLink);

        return

$@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>PremierVenue Password Reset</title>
</head>
<body style=""margin:0;padding:0;background-color:#ecf0f1;font-family:'Segoe UI',Tahoma,Geneva,Verdana,sans-serif;color:#2c3e50;"">
    <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""background-color:#ecf0f1;padding:24px 12px;"">
        <tr>
            <td align=""center"">
                <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""max-width:600px;background-color:#ffffff;border-radius:14px;overflow:hidden;box-shadow:0 12px 30px rgba(44,62,80,0.12);"">
                    <tr>
                        <td style=""background:linear-gradient(135deg,#4a90e2 0%,#2c3e50 100%);padding:28px 32px;color:#ffffff;"">
                            <h1 style=""margin:0;font-size:24px;line-height:1.3;font-weight:700;"">Password Reset Request</h1>
                            <p style=""margin:10px 0 0 0;font-size:14px;opacity:0.95;"">Secure your PremierVenue account</p>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding:30px 32px 20px 32px;"">
                            <p style=""margin:0 0 14px 0;font-size:16px;line-height:1.6;"">Hi {safeFirstName},</p>
                            <p style=""margin:0 0 16px 0;font-size:15px;line-height:1.7;"">Use the one-time code below to reset your password. For your security, this code expires in <strong>10 minutes</strong>.</p>
                            <div style=""margin:18px 0 24px 0;padding:16px;background-color:#f5f9ff;border:1px solid #d6e7fb;border-radius:12px;text-align:center;"">
                                <span style=""display:inline-block;font-size:32px;letter-spacing:8px;font-weight:700;color:#2c3e50;"">{safeOtp}</span>
                            </div>
                            <p style=""margin:0 0 18px 0;font-size:14px;line-height:1.7;color:#34495e;"">You can also continue directly from the reset page:</p>
                            <p style=""margin:0 0 20px 0;text-align:center;"">
                                <a href=""{safeResetLink}"" style=""display:inline-block;background-color:#4a90e2;color:#ffffff;text-decoration:none;padding:12px 22px;border-radius:10px;font-size:14px;font-weight:600;"">Reset Password</a>
                            </p>
                            <p style=""margin:0;font-size:13px;line-height:1.7;color:#5d6d7e;"">If you did not request this reset, you can safely ignore this email. Your existing password will remain unchanged.</p>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding:16px 32px 24px 32px;border-top:1px solid #ecf0f1;font-size:12px;line-height:1.6;color:#7f8c8d;"">
                            PremierVenue Security Team
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    private static string BuildStaffInvitationEmailBody(string firstName, string userName, string invitationLink)
    {
        var safeFirstName = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(firstName) ? "there" : firstName);
        var safeUserName = WebUtility.HtmlEncode(userName);
        var safeInvitationLink = WebUtility.HtmlEncode(invitationLink);

        return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>PremierVenue Staff Invitation</title>
</head>
<body style=""margin:0;padding:0;background-color:#ecf0f1;font-family:'Segoe UI',Tahoma,Geneva,Verdana,sans-serif;color:#2c3e50;"">
    <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""background-color:#ecf0f1;padding:24px 12px;"">
        <tr>
            <td align=""center"">
                <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""max-width:600px;background-color:#ffffff;border-radius:14px;overflow:hidden;box-shadow:0 12px 30px rgba(44,62,80,0.12);"">
                    <tr>
                        <td style=""background:linear-gradient(135deg,#4a90e2 0%,#2c3e50 100%);padding:28px 32px;color:#ffffff;"">
                            <h1 style=""margin:0;font-size:24px;line-height:1.3;font-weight:700;"">You're Invited to PremierVenue</h1>
                            <p style=""margin:10px 0 0 0;font-size:14px;opacity:0.95;"">Activate your staff account</p>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding:30px 32px 20px 32px;"">
                            <p style=""margin:0 0 14px 0;font-size:16px;line-height:1.6;"">Hi {safeFirstName},</p>
                            <p style=""margin:0 0 12px 0;font-size:15px;line-height:1.7;"">You have been invited to join PremierVenue as a staff member.</p>
                            <p style=""margin:0 0 16px 0;font-size:15px;line-height:1.7;"">Your username: <strong>{safeUserName}</strong></p>
                            <p style=""margin:0 0 20px 0;font-size:14px;line-height:1.7;color:#34495e;"">Click the button below to accept your invitation and set your password. This invitation expires in <strong>7 days</strong>.</p>
                            <p style=""margin:0 0 18px 0;text-align:center;"">
                                <a href=""{safeInvitationLink}"" style=""display:inline-block;background-color:#4a90e2;color:#ffffff;text-decoration:none;padding:12px 24px;border-radius:10px;font-size:14px;font-weight:600;"">Accept Invitation</a>
                            </p>
                            <p style=""margin:0 0 8px 0;font-size:13px;line-height:1.7;color:#5d6d7e;"">If the button does not work, copy and paste this link into your browser:</p>
                            <p style=""margin:0;font-size:12px;line-height:1.8;color:#2c3e50;word-break:break-all;"">{safeInvitationLink}</p>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding:16px 32px 24px 32px;border-top:1px solid #ecf0f1;font-size:12px;line-height:1.6;color:#7f8c8d;"">
                            PremierVenue Team
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    private async Task<AuthResponseDto> GenerateAuthResponseAsync(User user)
    {
        var secret = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
        var issuer = _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");

        var token = JwtTokenGenerator.GenerateToken(user.Id.ToString(), user.Email, user.Role.ToString(), secret, issuer, 60);
        var refreshToken = JwtTokenGenerator.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            User = MapToUserDto(user)
        };
    }

    private static UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.UserName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role.ToString(),
            Status = ResolveStatus(user),
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }

    private static string ResolveStatus(User user)
    {
        if (string.IsNullOrEmpty(user.PasswordHash))
            return "Pending";

        return user.IsActive ? "Active" : "Inactive";
    }
}
