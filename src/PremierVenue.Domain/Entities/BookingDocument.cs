namespace PremierVenue.Domain.Entities;

public class BookingDocument
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public DocumentType DocumentType { get; set; }
    public long FileSize { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Booking Booking { get; set; } = null!;
}

public enum DocumentType
{
    Contract = 1,
    Invoice = 2,
    Quote = 3,
    Receipt = 4,
    Insurance = 5,
    ProofOfPayment = 6,
    Other = 7
}