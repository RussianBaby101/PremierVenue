namespace PremierVenue.Domain.Entities;

public class Message
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public int SenderId { get; set; }
    public int? ReceiverId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }

    // Navigation properties
    public Booking Booking { get; set; } = null!;
    public User Sender { get; set; } = null!;
    public User? Receiver { get; set; }
}