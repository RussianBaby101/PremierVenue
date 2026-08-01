using PremierVenue.Domain.Enums;

namespace PremierVenue.Domain.Entities;

public class VenueEventType
{
    public int VenueId { get; set; }
    public EventType EventType { get; set; }
    public Venue Venue { get; set; } = null!;
}
