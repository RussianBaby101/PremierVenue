using PremierVenue.Domain.Entities;
using PremierVenue.Domain.Enums;

namespace PremierVenue.Domain.Interfaces;

public interface IVenueRepository : IRepository<Venue>
{
    Task<IEnumerable<Venue>> GetActiveVenuesAsync();
    Task<IEnumerable<Venue>> SearchVenuesAsync(string searchTerm, int? capacity, decimal? minPrice, decimal? maxPrice, EventType? eventType = null, string? sortBy = null);
    Task<Venue?> GetVenueWithDetailsAsync(int id);
    Task<IEnumerable<Venue>> GetAllWithDetailsAsync(bool includeInactive, string? sortBy = null);
    Task<(byte[] Content, string ContentType, string FileName)?> GetPhotoContentAsync(int venueId, int photoId);
}