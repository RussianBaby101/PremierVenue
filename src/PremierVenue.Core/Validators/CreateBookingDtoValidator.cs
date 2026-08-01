using System;
using FluentValidation;
using PremierVenue.Core.DTOs;

namespace PremierVenue.Core.Validators;

public class CreateBookingDtoValidator : AbstractValidator<CreateBookingDto>
{
    public CreateBookingDtoValidator()
    {
        RuleFor(x => x.VenueId)
            .GreaterThan(0).WithMessage("Valid venue ID is required");

        RuleFor(x => x.EventType)
            .NotEmpty().WithMessage("Event type is required")
            .Must(BeAValidEventType).WithMessage("Invalid event type");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required")
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("Start date must be today or in the future");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("End date must be on or after start date");

        RuleFor(x => x.ExpectedGuests)
            .GreaterThan(0).WithMessage("Expected guests must be greater than 0");

        RuleFor(x => x.SpecialRequirements)
            .MaximumLength(2000).WithMessage("Special requirements cannot exceed 2000 characters");

        RuleFor(x => x.AdditionalServices)
            .MaximumLength(2000).WithMessage("Additional services cannot exceed 2000 characters");
    }

    private bool BeAValidEventType(string eventType)
    {
        var validTypes = new[] { "Wedding", "Corporate", "Birthday", "Conference", "Exhibition", "Concert", "PrivateParty", "Workshop", "Seminar", "Other" };
        return validTypes.Contains(eventType);
    }
}