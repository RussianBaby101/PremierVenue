namespace PremierVenue.Core.DTOs;

public class BookingDto
{
    public int Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public int VenueId { get; set; }
    public string VenueName { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int ExpectedGuests { get; set; }
    public string SpecialRequirements { get; set; } = string.Empty;
    public bool CateringRequested { get; set; }
    public bool StaffingSecurityRequested { get; set; }
    public bool SetupCleanupRequested { get; set; }
    public string AdditionalServices { get; set; } = string.Empty;
    public decimal FinalQuote { get; set; }
    public decimal DepositAmount { get; set; }
    public decimal BalanceAmount { get; set; }
    public DateTime? QuoteExpiresAt { get; set; }
    public string CancellationPolicy { get; set; } = string.Empty;
    public string CancellationPolicyCode { get; set; } = "Standard";
    public decimal RefundAmount { get; set; }
    public string RefundStatus { get; set; } = "NotApplicable";
    public decimal CancellationFeeAmount { get; set; }
    public string CancellationFeeStatus { get; set; } = "NotApplicable";
    public DateTime? CancellationFeeDueAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? InternalNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<PaymentDto> Payments { get; set; } = new();
    public List<BookingDocumentDto> Documents { get; set; } = new();
}

public class CreateBookingDto
{
    public int VenueId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int ExpectedGuests { get; set; }
    public string SpecialRequirements { get; set; } = string.Empty;
    public bool CateringRequested { get; set; }
    public bool StaffingSecurityRequested { get; set; }
    public bool SetupCleanupRequested { get; set; }
    public string AdditionalServices { get; set; } = string.Empty;
}

public class UpdateBookingDto
{
    public string EventType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int ExpectedGuests { get; set; }
    public string SpecialRequirements { get; set; } = string.Empty;
    public bool CateringRequested { get; set; }
    public bool StaffingSecurityRequested { get; set; }
    public bool SetupCleanupRequested { get; set; }
    public string AdditionalServices { get; set; } = string.Empty;
    public decimal FinalQuote { get; set; }
    public decimal DepositAmount { get; set; }
    public DateTime? QuoteExpiresAt { get; set; }
    public string CancellationPolicy { get; set; } = string.Empty;
    public string CancellationPolicyCode { get; set; } = "Standard";
    public decimal RefundAmount { get; set; }
    public string RefundStatus { get; set; } = "NotApplicable";
    public decimal CancellationFeeAmount { get; set; }
    public string CancellationFeeStatus { get; set; } = "NotApplicable";
    public DateTime? CancellationFeeDueAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? InternalNotes { get; set; }
}

public class BookingQuoteDto
{
    public int BookingId { get; set; }
    public decimal FinalQuote { get; set; }
    public decimal DepositAmount { get; set; }
    public DateTime? QuoteExpiresAt { get; set; }
    public string CancellationPolicy { get; set; } = string.Empty;
    public string CancellationPolicyCode { get; set; } = "Standard";
    public string? Notes { get; set; }
}

public class BookingStatusUpdateDto
{
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class QuoteDecisionDto
{
    public bool Accepted { get; set; }
    public string? Notes { get; set; }
}