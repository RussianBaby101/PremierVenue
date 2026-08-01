using PremierVenue.Domain.Enums;

namespace PremierVenue.Core.DTOs;

public class SavedVenueDto
{
    public int VenueId { get; set; }
    public DateTime SavedAt { get; set; }
    public VenueDto Venue { get; set; } = null!;
}

public class AvailabilityDto
{
    public DateTime Date { get; set; }
    public bool IsAvailable { get; set; }
    public string? Notes { get; set; }
}

public class EventTypeDto
{
    public EventType Value { get; set; }
    public string Name { get; set; } = string.Empty;
}
