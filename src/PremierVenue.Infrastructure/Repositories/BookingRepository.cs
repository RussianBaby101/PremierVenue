using Microsoft.EntityFrameworkCore;
using PremierVenue.Domain.Entities;
using PremierVenue.Domain.Enums;
using PremierVenue.Domain.Interfaces;
using PremierVenue.Infrastructure.Data;

namespace PremierVenue.Infrastructure.Repositories;

// Booking repository with queries for client, pending, date range, and reference number lookups
public class BookingRepository : Repository<Booking>, IBookingRepository
{
    public BookingRepository(AppDbContext context) : base(context)
    {
    }

    public new async Task<IEnumerable<Booking>> GetAllAsync()
    {
        return await _dbSet
            .Include(b => b.Client)
            .Include(b => b.Venue)
            .Include(b => b.Documents)
            .Include(b => b.Payments)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<Booking?> GetByReferenceNumberAsync(string referenceNumber)
    {
        return await _dbSet
            .Include(b => b.Client)
            .Include(b => b.Venue)
            .Include(b => b.Documents)
            .Include(b => b.Payments)
            .Include(b => b.Documents)
            .Include(b => b.Messages)
            .Include(b => b.Tasks)
            .FirstOrDefaultAsync(b => b.ReferenceNumber == referenceNumber);
    }

    public async Task<IEnumerable<Booking>> GetClientBookingsAsync(int clientId)
    {
        return await _dbSet
            .Where(b => b.ClientId == clientId)
            .Include(b => b.Venue)
            .Include(b => b.Documents)
            .Include(b => b.Payments)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Booking>> GetPendingBookingsAsync()
    {
        return await _dbSet
            .Where(b => b.Status == BookingStatus.Pending)
            .Include(b => b.Client)
            .Include(b => b.Venue)
            .OrderBy(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Booking>> GetBookingsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(b => b.StartDate >= startDate && b.EndDate <= endDate)
            .Include(b => b.Client)
            .Include(b => b.Venue)
            .ToListAsync();
    }
}