using FluentValidation;
using PremierVenue.Core.DTOs;

namespace PremierVenue.Core.Validators;

public class BookingStatusUpdateDtoValidator : AbstractValidator<BookingStatusUpdateDto>
{
    private static readonly string[] ValidStatuses =
    {
        "Pending", "Quoted", "QuoteAccepted", "QuoteRejected", "Confirmed", "DepositPaid",
        "FullyPaid", "Cancelled", "Rejected"
    };

    public BookingStatusUpdateDtoValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .Must(BeAValidStatus).WithMessage("Invalid booking status");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters");
    }

    private bool BeAValidStatus(string status)
    {
        return ValidStatuses.Contains(status);
    }
}