namespace PremierVenue.Domain.Entities;

public class Venue : BaseEntity
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
    public string? ImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public List<string> CustomAmenities { get; set; } = new();
    public List<string> SupportedServices { get; set; } = new();

    // Navigation properties
    public ICollection<VenueAmenity> Amenities { get; set; } = new List<VenueAmenity>();
    public ICollection<VenuePhoto> Photos { get; set; } = new List<VenuePhoto>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Availability> Availabilities { get; set; } = new List<Availability>();
    public ICollection<VenueEventType> EventTypes { get; set; } = new List<VenueEventType>();
    public ICollection<SavedVenue> SavedByUsers { get; set; } = new List<SavedVenue>();
}