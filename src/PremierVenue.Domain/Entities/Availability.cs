namespace PremierVenue.Domain.Entities;

public class Availability : BaseEntity
{
    public int VenueId { get; set; }
    public DateTime Date { get; set; }
    public bool IsAvailable { get; set; } = true;
    public string? Notes { get; set; }

    // Navigation properties
    public Venue Venue { get; set; } = null!;
}