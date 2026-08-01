namespace PremierVenue.Domain.Enums;

public enum BookingStatus
{
    Pending = 1,
    Quoted = 2,
    QuoteAccepted = 3,
    QuoteRejected = 10,
    Confirmed = 4,
    DepositPaid = 5,
    FullyPaid = 6,
    Completed = 7,
    Cancelled = 8,
    Rejected = 9
}