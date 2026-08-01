using Microsoft.EntityFrameworkCore;
using PremierVenue.Domain.Entities;
using PremierVenue.Domain.Enums;
using PremierVenue.Domain.Interfaces;
using PremierVenue.Infrastructure.Data;

namespace PremierVenue.Infrastructure.Repositories;

// Repository for venue data, including search, filters, and photo content retrieval
public class VenueRepository : Repository<Venue>, IVenueRepository
{
    public VenueRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Venue>> GetActiveVenuesAsync()
    {
        var venues = await _dbSet
            .AsNoTracking()
            .Where(v => v.IsActive)
            .Include(v => v.Amenities)
            .ThenInclude(va => va.Amenity)
            .Include(v => v.EventTypes)
            .ToListAsync();
        await LoadPhotoMetadataAsync(venues, trackMetadata: false);
        return venues;
    }

    public async Task<IEnumerable<Venue>> SearchVenuesAsync(string searchTerm, int? capacity, decimal? minPrice, decimal? maxPrice, EventType? eventType = null, string? sortBy = null)
    {
        var query = _dbSet.Where(v => v.IsActive);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(v => v.Name.Contains(searchTerm) ||
                                     v.City.Contains(searchTerm) ||
                                     v.Description.Contains(searchTerm));
        }

        if (capacity.HasValue)
            query = query.Where(v => v.Capacity >= capacity.Value);
        if (minPrice.HasValue)
            query = query.Where(v => v.BasePricePerDay >= minPrice.Value);
        if (maxPrice.HasValue)
            query = query.Where(v => v.BasePricePerDay <= maxPrice.Value);
        if (eventType.HasValue)
            query = query.Where(v => v.EventTypes.Any(eventTypeLink => eventTypeLink.EventType == eventType.Value));

        query = sortBy?.ToLowerInvariant() switch
        {
            "price-asc" => query.OrderBy(v => v.BasePricePerDay),
            "price-desc" => query.OrderByDescending(v => v.BasePricePerDay),
            "capacity" => query.OrderByDescending(v => v.Capacity),
            "newest" => query.OrderByDescending(v => v.CreatedAt),
            _ => query.OrderBy(v => v.Name)
        };

        var venues = await query
            .AsNoTracking()
            .Include(v => v.Amenities)
            .ThenInclude(va => va.Amenity)
            .Include(v => v.EventTypes)
            .ToListAsync();
        await LoadPhotoMetadataAsync(venues, trackMetadata: false);
        return venues;
    }

    public async Task<IEnumerable<Venue>> GetAllWithDetailsAsync(bool includeInactive, string? sortBy = null)
    {
        var query = _dbSet.AsQueryable();
        if (!includeInactive)
            query = query.Where(v => v.IsActive);

        query = sortBy?.ToLowerInvariant() switch
        {
            "price-asc" => query.OrderBy(v => v.BasePricePerDay),
            "price-desc" => query.OrderByDescending(v => v.BasePricePerDay),
            "capacity" => query.OrderByDescending(v => v.Capacity),
            "newest" => query.OrderByDescending(v => v.CreatedAt),
            _ => query.OrderBy(v => v.Name)
        };

        var venues = await query
            .AsNoTracking()
            .Include(v => v.Amenities)
            .ThenInclude(va => va.Amenity)
            .Include(v => v.EventTypes)
            .ToListAsync();
        await LoadPhotoMetadataAsync(venues, trackMetadata: false);
        return venues;
    }

    public async Task<Venue?> GetVenueWithDetailsAsync(int id)
    {
        var venue = await _dbSet
            .Include(v => v.Amenities)
            .ThenInclude(va => va.Amenity)
            .Include(v => v.Availabilities)
            .Include(v => v.Bookings)
            .Include(v => v.EventTypes)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (venue != null)
            await LoadPhotoMetadataAsync(new[] { venue }, trackMetadata: true);
        return venue;
    }

    public async Task<(byte[] Content, string ContentType, string FileName)?> GetPhotoContentAsync(int venueId, int photoId)
    {
        var photo = await _context.VenuePhotos
            .AsNoTracking()
            .Where(item => item.VenueId == venueId && item.Id == photoId)
            .Select(item => new { item.Content, item.ContentType, item.FileName })
            .FirstOrDefaultAsync();

        return photo?.Content == null
            ? null
            : (photo.Content, photo.ContentType ?? "application/octet-stream", photo.FileName ?? "venue-image");
    }

    private async Task LoadPhotoMetadataAsync(IEnumerable<Venue> venues, bool trackMetadata)
    {
        var venueIds = venues.Select(venue => venue.Id).ToList();
        if (venueIds.Count == 0)
            return;

        var photos = await _context.VenuePhotos
            .AsNoTracking()
            .Where(photo => venueIds.Contains(photo.VenueId))
            .Select(photo => new VenuePhoto
            {
                Id = photo.Id,
                VenueId = photo.VenueId,
                Url = photo.Url,
                Caption = photo.Caption,
                FileName = photo.FileName,
                ContentType = photo.ContentType,
                DisplayOrder = photo.DisplayOrder,
                IsPrimary = photo.IsPrimary
            })
            .ToListAsync();

        foreach (var venue in venues)
        {
            var metadata = photos.Where(photo => photo.VenueId == venue.Id).ToList();
            if (trackMetadata)
                _context.AttachRange(metadata);
            venue.Photos = metadata;
        }
    }
}
