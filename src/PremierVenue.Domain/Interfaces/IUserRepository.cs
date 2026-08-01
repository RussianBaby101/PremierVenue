using PremierVenue.Domain.Entities;

namespace PremierVenue.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByRefreshTokenAsync(string refreshToken);
    Task<User?> GetByInvitationTokenAsync(string token);
}