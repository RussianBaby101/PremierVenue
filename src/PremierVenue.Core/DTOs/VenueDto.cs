using PremierVenue.Domain.Enums;

namespace PremierVenue.Core.DTOs;

public class VenueDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public int Capacity { get; set; }
    public decimal BasePricePerDay { get; set; }
    public string? ImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<AmenityDto> Amenities { get; set; } = new();
    public List<string> CustomAmenities { get; set; } = new();
    public List<string> SupportedServices { get; set; } = new();
    public List<VenuePhotoDto> Photos { get; set; } = new();
    public List<EventType> EventTypes { get; set; } = new();
    public List<AvailabilityDto> Availabilities { get; set; } = new();
}

public class CreateVenueDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public int Capacity { get; set; }
    public decimal BasePricePerDay { get; set; }
    public bool IsFeatured { get; set; }
    public string? ImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public List<int> AmenityIds { get; set; } = new();
    public List<string> CustomAmenities { get; set; } = new();
    public List<string> SupportedServices { get; set; } = new();
    public List<EventType> EventTypes { get; set; } = new();
}

public class UpdateVenueDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public int Capacity { get; set; }
    public decimal BasePricePerDay { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public string? ImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public List<int> AmenityIds { get; set; } = new();
    public List<string> CustomAmenities { get; set; } = new();
    public List<string> SupportedServices { get; set; } = new();
    public List<EventType> EventTypes { get; set; } = new();
}

public class VenueSearchDto
{
    public string? SearchTerm { get; set; }
    public int? Capacity { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? City { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public EventType? EventType { get; set; }
    public string? SortBy { get; set; }
}

public class AmenityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}

public class VenuePhotoDto
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }
}