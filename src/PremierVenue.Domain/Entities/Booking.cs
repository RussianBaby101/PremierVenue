using PremierVenue.Domain.Enums;

namespace PremierVenue.Domain.Entities;

public class Booking
{
    public int Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public int ClientId { get; set; }
    public int VenueId { get; set; }
    public EventType EventType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int ExpectedGuests { get; set; }
    public string SpecialRequirements { get; set; } = string.Empty;
    public bool CateringRequested { get; set; }
    public bool StaffingSecurityRequested { get; set; }
    public bool SetupCleanupRequested { get; set; }
    public string AdditionalServices { get; set; } = string.Empty;
    public decimal EstimatedBudget { get; set; }
    public decimal FinalQuote { get; set; }
    public decimal DepositAmount { get; set; }
    public DateTime? QuoteExpiresAt { get; set; }
    public string CancellationPolicy { get; set; } = string.Empty;
    public string CancellationPolicyCode { get; set; } = "Standard";
    public decimal BalanceAmount { get; set; }
    public decimal RefundAmount { get; set; }
    public string RefundStatus { get; set; } = "NotApplicable";
    public decimal CancellationFeeAmount { get; set; }
    public string CancellationFeeStatus { get; set; } = "NotApplicable";
    public DateTime? CancellationFeeDueAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public BookingStatus Status { get; set; }
    public string? InternalNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    public User Client { get; set; } = null!;
    public Venue Venue { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<BookingDocument> Documents { get; set; } = new List<BookingDocument>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}