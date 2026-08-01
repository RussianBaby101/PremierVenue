using Microsoft.EntityFrameworkCore;
using PremierVenue.Domain.Entities;
using PremierVenue.Domain.Interfaces;
using PremierVenue.Infrastructure.Data;

namespace PremierVenue.Infrastructure.Repositories;

// Repository for user accounts, with lookups by email, refresh token, and invitation token
public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken && u.RefreshTokenExpiry > DateTime.UtcNow);
    }

    public async Task<User?> GetByInvitationTokenAsync(string token)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.InvitationToken == token);
    }
}