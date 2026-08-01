using FluentValidation;
using PremierVenue.Core.DTOs;
using PremierVenue.Core.Utilities;

namespace PremierVenue.Core.Validators;

public class CreateStaffInvitationDtoValidator : AbstractValidator<CreateStaffInvitationDto>
{
    public CreateStaffInvitationDtoValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .MaximumLength(200).WithMessage("Full name cannot exceed 200 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .Must(EmailHelper.IsValidEmail).WithMessage("Invalid email format");
    }
}
