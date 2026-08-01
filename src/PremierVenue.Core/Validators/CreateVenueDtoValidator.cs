using FluentValidation;
using PremierVenue.Core.DTOs;

namespace PremierVenue.Core.Validators;

public class CreateVenueDtoValidator : AbstractValidator<CreateVenueDto>
{
    public CreateVenueDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Venue name is required")
            .MaximumLength(200).WithMessage("Venue name cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required")
            .MaximumLength(500).WithMessage("Address cannot exceed 500 characters");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required")
            .MaximumLength(100).WithMessage("City cannot exceed 100 characters");

        RuleFor(x => x.Province)
            .NotEmpty().WithMessage("Province is required")
            .MaximumLength(100).WithMessage("Province cannot exceed 100 characters");

        RuleFor(x => x.PostalCode)
            .NotEmpty().WithMessage("Postal code is required")
            .MaximumLength(20).WithMessage("Postal code cannot exceed 20 characters");

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Capacity must be greater than 0");

        RuleFor(x => x.BasePricePerDay)
            .GreaterThan(0).WithMessage("Base price per day must be greater than 0");

        RuleFor(x => x.ImageUrl)
            .Must(uri => string.IsNullOrWhiteSpace(uri) || Uri.TryCreate(uri, UriKind.RelativeOrAbsolute, out _))
            .WithMessage("Image URL must be a valid URL or empty");

        RuleFor(x => x.ThumbnailUrl)
            .Must(uri => string.IsNullOrWhiteSpace(uri) || Uri.TryCreate(uri, UriKind.RelativeOrAbsolute, out _))
            .WithMessage("Thumbnail URL must be a valid URL or empty");

        RuleForEach(x => x.CustomAmenities)
            .MaximumLength(100).WithMessage("Custom amenity names cannot exceed 100 characters");

        RuleForEach(x => x.SupportedServices)
            .MaximumLength(100).WithMessage("Service option names cannot exceed 100 characters");
    }
}