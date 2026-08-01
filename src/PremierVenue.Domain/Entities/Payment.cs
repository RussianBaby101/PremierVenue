using PremierVenue.Domain.Enums;

namespace PremierVenue.Domain.Entities;

public class Payment
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public decimal Amount { get; set; }
    public PaymentType PaymentType { get; set; }
    public PaymentStatus Status { get; set; }
    public string? TransactionReference { get; set; }
    public string? PaymentGatewayResponse { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }

    // Navigation properties
    public Booking Booking { get; set; } = null!;
}

public enum PaymentType
{
    Deposit = 1,
    FullPayment = 2,
    PartialPayment = 3,
    Refund = 4
}

public enum PaymentStatus
{
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4,
    Refunded = 5
}