using PremierVenue.Domain.Entities;

namespace PremierVenue.Domain.Interfaces;

public interface IBookingRepository : IRepository<Booking>
{
    Task<Booking?> GetByReferenceNumberAsync(string referenceNumber);
    Task<IEnumerable<Booking>> GetClientBookingsAsync(int clientId);
    Task<IEnumerable<Booking>> GetPendingBookingsAsync();
    Task<IEnumerable<Booking>> GetBookingsByDateRangeAsync(DateTime startDate, DateTime endDate);
}