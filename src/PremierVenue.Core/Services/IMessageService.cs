using PremierVenue.Core.DTOs;
namespace PremierVenue.Core.Services;

public interface IMessageService
{
    Task<MessageDto?> GetMessageByIdAsync(int id);
    Task<List<MessageDto>> GetBookingMessagesAsync(int bookingId);
    Task<MessageDto?> CreateMessageAsync(CreateMessageDto model, int senderId);
    Task<bool> MarkAsReadAsync(int messageId, int userId);
    Task<int> GetUnreadCountAsync(int userId);
}