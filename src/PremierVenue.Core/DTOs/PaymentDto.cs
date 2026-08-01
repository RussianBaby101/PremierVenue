namespace PremierVenue.Core.DTOs;

public class PaymentDto
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? TransactionReference { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}

public class CreatePaymentDto
{
    public int BookingId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentType { get; set; } = string.Empty;
    public string? PaymentMethod { get; set; }
    public string? CallbackUrl { get; set; }
}

public class PaymentResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? PaymentUrl { get; set; }
    public string? TransactionReference { get; set; }
}