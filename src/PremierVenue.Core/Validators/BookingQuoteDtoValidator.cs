using FluentValidation;
using PremierVenue.Core.DTOs;

namespace PremierVenue.Core.Validators;

public class BookingQuoteDtoValidator : AbstractValidator<BookingQuoteDto>
{
    public BookingQuoteDtoValidator()
    {
        RuleFor(x => x.BookingId)
            .GreaterThan(0).WithMessage("Valid booking ID is required");

        RuleFor(x => x.FinalQuote)
            .GreaterThan(0).WithMessage("Final quote must be greater than 0");

        RuleFor(x => x.DepositAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Deposit amount must be 0 or greater");

        RuleFor(x => x.QuoteExpiresAt)
            .GreaterThan(DateTime.UtcNow).When(x => x.QuoteExpiresAt.HasValue)
            .WithMessage("Quote expiry must be in the future");

        RuleFor(x => x.CancellationPolicy)
            .NotEmpty().WithMessage("Cancellation policy is required")
            .MaximumLength(4000).WithMessage("Cancellation policy cannot exceed 4000 characters");

        RuleFor(x => x.CancellationPolicyCode)
            .Must(code => new[] { "Standard", "FullRefund", "NoRefund" }.Contains(code))
            .WithMessage("Invalid cancellation policy");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters");
    }
}