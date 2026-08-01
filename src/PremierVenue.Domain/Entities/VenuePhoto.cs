namespace PremierVenue.Domain.Entities;

public class VenuePhoto : BaseEntity
{
    public int VenueId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
    public byte[]? Content { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; } = false;

    // Navigation properties
    public Venue Venue { get; set; } = null!;
}