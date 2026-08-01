namespace PremierVenue.Domain.Entities;

public class VenueAmenity : BaseEntity
{
    public int VenueId { get; set; }
    public int AmenityId { get; set; }
    public bool IsIncluded { get; set; } = true;
    public decimal? AdditionalCost { get; set; }

    // Navigation properties
    public Venue Venue { get; set; } = null!;
    public Amenity Amenity { get; set; } = null!;
}