namespace PremierVenue.Core.DTOs;

public class BookingDocumentDto
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UploadDocumentDto
{
    public int BookingId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string? Description { get; set; }
}