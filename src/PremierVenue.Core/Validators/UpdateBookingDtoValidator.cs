using FluentValidation;
using PremierVenue.Core.DTOs;

namespace PremierVenue.Core.Validators;

public class UpdateBookingDtoValidator : AbstractValidator<UpdateBookingDto>
{
    private static readonly string[] ValidEventTypes =
    {
        "Wedding", "Corporate", "Birthday", "Conference", "Exhibition",
        "Concert", "PrivateParty", "Workshop", "Seminar", "Other"
    };

    private static readonly string[] ValidStatuses =
    {
        "Pending", "Quoted", "QuoteAccepted", "QuoteRejected", "Confirmed", "DepositPaid",
        "FullyPaid", "Completed", "Cancelled", "Rejected"
    };

    public UpdateBookingDtoValidator()
    {
        RuleFor(x => x.EventType)
            .NotEmpty().WithMessage("Event type is required")
            .Must(BeAValidEventType).WithMessage("Invalid event type");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("End date must be on or after start date");

        RuleFor(x => x.ExpectedGuests)
            .GreaterThan(0).WithMessage("Expected guests must be greater than 0");

        RuleFor(x => x.FinalQuote)
            .GreaterThanOrEqualTo(0).WithMessage("Final quote must be 0 or greater");

        RuleFor(x => x.DepositAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Deposit amount must be 0 or greater");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .Must(BeAValidStatus).WithMessage("Invalid booking status");

        RuleFor(x => x.SpecialRequirements)
            .MaximumLength(2000).WithMessage("Special requirements cannot exceed 2000 characters");

        RuleFor(x => x.AdditionalServices)
            .MaximumLength(2000).WithMessage("Additional services cannot exceed 2000 characters");

        RuleFor(x => x.InternalNotes)
            .MaximumLength(2000).WithMessage("Internal notes cannot exceed 2000 characters");
    }

    private bool BeAValidEventType(string eventType)
    {
        return ValidEventTypes.Contains(eventType);
    }

    private bool BeAValidStatus(string status)
    {
        return ValidStatuses.Contains(status);
    }
}