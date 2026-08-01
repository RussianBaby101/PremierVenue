using PremierVenue.Core.DTOs;
namespace PremierVenue.Core.Services;

public interface IBookingService
{
    Task<BookingDto?> GetBookingByIdAsync(int id);
    Task<BookingDto?> GetBookingByReferenceNumberAsync(string referenceNumber);
    Task<PagedResponseDto<BookingDto>> GetAllBookingsAsync(int page = 1, int pageSize = 10);
    Task<PagedResponseDto<BookingDto>> GetClientBookingsAsync(int clientId, int page = 1, int pageSize = 10);
    Task<PagedResponseDto<BookingDto>> GetPendingBookingsAsync(int page = 1, int pageSize = 10);
    Task<BookingDto?> CreateBookingAsync(CreateBookingDto model, int clientId);
    Task<BookingDto?> UpdateBookingAsync(int id, UpdateBookingDto model);
    Task<BookingDto?> UpdateBookingStatusAsync(int id, BookingStatusUpdateDto model);
    Task<BookingDto?> SendQuoteAsync(BookingQuoteDto model);
    Task<BookingDto?> DecideQuoteAsync(int bookingId, QuoteDecisionDto model, int clientId);
    Task<bool> DeleteBookingAsync(int id);
}