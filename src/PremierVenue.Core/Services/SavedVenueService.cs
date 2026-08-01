using PremierVenue.Core.DTOs;
using PremierVenue.Domain.Entities;
using PremierVenue.Domain.Interfaces;

namespace PremierVenue.Core.Services;

public class SavedVenueService : ISavedVenueService
{
    private readonly ISavedVenueRepository _savedVenueRepository;
    private readonly IVenueRepository _venueRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SavedVenueService(
        ISavedVenueRepository savedVenueRepository,
        IVenueRepository venueRepository,
        IUnitOfWork unitOfWork)
    {
        _savedVenueRepository = savedVenueRepository;
        _venueRepository = venueRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<SavedVenueDto>> GetSavedVenuesAsync(int userId)
    {
        return (await _savedVenueRepository.GetByUserIdAsync(userId))
            .Where(saved => saved.Venue.IsActive)
            .Select(saved => new SavedVenueDto
            {
                VenueId = saved.VenueId,
                SavedAt = saved.CreatedAt,
                Venue = MapVenue(saved.Venue)
            })
            .ToList();
    }

    public async Task<SavedVenueDto?> SaveAsync(int userId, int venueId)
    {
        var venue = await _venueRepository.GetVenueWithDetailsAsync(venueId);
        if (venue == null || !venue.IsActive)
        {
            return null;
        }

        var existing = await _savedVenueRepository.GetAsync(userId, venueId);
        if (existing != null)
        {
            return new SavedVenueDto { VenueId = venueId, SavedAt = existing.CreatedAt, Venue = MapVenue(venue) };
        }

        var saved = new SavedVenue { UserId = userId, VenueId = venueId };
        await _savedVenueRepository.AddAsync(saved);
        await _unitOfWork.SaveChangesAsync();
        return new SavedVenueDto { VenueId = venueId, SavedAt = saved.CreatedAt, Venue = MapVenue(venue) };
    }

    public async Task<bool> RemoveAsync(int userId, int venueId)
    {
        var saved = await _savedVenueRepository.GetAsync(userId, venueId);
        if (saved == null)
        {
            return false;
        }

        _savedVenueRepository.Delete(saved);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsSavedAsync(int userId, int venueId)
    {
        return await _savedVenueRepository.GetAsync(userId, venueId) != null;
    }

    private static VenueDto MapVenue(Venue venue)
    {
        return new VenueDto
        {
            Id = venue.Id,
            Name = venue.Name,
            Description = venue.Description,
            Address = venue.Address,
            City = venue.City,
            Province = venue.Province,
            PostalCode = venue.PostalCode,
            Latitude = venue.Latitude,
            Longitude = venue.Longitude,
            Capacity = venue.Capacity,
            BasePricePerDay = venue.BasePricePerDay,
            ImageUrl = venue.ImageUrl,
            ThumbnailUrl = venue.ThumbnailUrl,
            IsActive = venue.IsActive,
            CreatedAt = venue.CreatedAt,
            Amenities = venue.Amenities.Select(va => new AmenityDto
            {
                Id = va.Amenity.Id,
                Name = va.Amenity.Name,
                Description = va.Amenity.Description,
                Icon = va.Amenity.Icon
            }).ToList(),
            Photos = venue.Photos.OrderBy(photo => photo.DisplayOrder).Select(photo => new VenuePhotoDto
            {
                Id = photo.Id,
                Url = photo.Url,
                Caption = photo.Caption,
                DisplayOrder = photo.DisplayOrder,
                IsPrimary = photo.IsPrimary
            }).ToList(),
            EventTypes = venue.EventTypes.Select(eventType => eventType.EventType).OrderBy(eventType => eventType).ToList()
        };
    }
}
