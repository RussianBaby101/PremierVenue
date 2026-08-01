namespace PremierVenue.Domain.Entities;

public class SavedVenue
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int VenueId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Venue Venue { get; set; } = null!;
}
