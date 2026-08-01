namespace PremierVenue.Core.DTOs;

public class MessageDto
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public int SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public int? ReceiverId { get; set; }
    public string? ReceiverName { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}

public class CreateMessageDto
{
    public int BookingId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int? ReceiverId { get; set; }
}