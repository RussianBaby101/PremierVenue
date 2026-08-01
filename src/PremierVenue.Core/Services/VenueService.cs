using PremierVenue.Core.DTOs;
using PremierVenue.Domain.Entities;
using PremierVenue.Domain.Interfaces;
using PremierVenue.Domain.Enums;

namespace PremierVenue.Core.Services;

public class VenueService : IVenueService
{
    private readonly IVenueRepository _venueRepository;
    private readonly IUnitOfWork _unitOfWork;

    public VenueService(IVenueRepository venueRepository, IUnitOfWork unitOfWork)
    {
        _venueRepository = venueRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<VenueDto?> GetVenueByIdAsync(int id)
    {
        var venue = await _venueRepository.GetVenueWithDetailsAsync(id);
        if (venue == null)
            return null;

        return MapToVenueDto(venue);
    }

    public async Task<PagedResponseDto<VenueDto>> GetAllVenuesAsync(int page = 1, int pageSize = 10, bool includeInactive = false, string? sortBy = null)
    {
        var allVenues = await _venueRepository.GetAllWithDetailsAsync(includeInactive, sortBy);
        var venues = allVenues.ToList();

        var totalCount = venues.Count;
        var pagedVenues = venues
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var venueDtos = pagedVenues.Select(MapToVenueDto).ToList();

        return PagedResponseDto<VenueDto>.Create(venueDtos, page, pageSize, totalCount);
    }

    public async Task<PagedResponseDto<VenueDto>> SearchVenuesAsync(VenueSearchDto searchDto, int page = 1, int pageSize = 10)
    {
        var venues = await _venueRepository.SearchVenuesAsync(
            searchDto.SearchTerm ?? string.Empty,
            searchDto.Capacity,
            searchDto.MinPrice,
            searchDto.MaxPrice,
            searchDto.EventType,
            searchDto.SortBy
        );

        // Additional filtering for city if provided
        if (!string.IsNullOrWhiteSpace(searchDto.City))
        {
            venues = venues.Where(v => v.City.Equals(searchDto.City, StringComparison.OrdinalIgnoreCase));
        }

        var venueList = venues.ToList();
        var totalCount = venueList.Count;
        var pagedVenues = venueList
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var venueDtos = pagedVenues.Select(MapToVenueDto).ToList();

        return PagedResponseDto<VenueDto>.Create(venueDtos, page, pageSize, totalCount);
    }

    public async Task<VenueDto?> CreateVenueAsync(CreateVenueDto model)
    {
        var venue = new Venue
        {
            Name = model.Name,
            Description = model.Description,
            Address = model.Address,
            City = model.City,
            Province = model.Province,
            PostalCode = model.PostalCode,
            Latitude = model.Latitude,
            Longitude = model.Longitude,
            Capacity = model.Capacity,
            BasePricePerDay = model.BasePricePerDay,
            ImageUrl = model.ImageUrl,
            ThumbnailUrl = model.ThumbnailUrl,
            CustomAmenities = NormalizeCustomAmenities(model.CustomAmenities),
            SupportedServices = NormalizeSupportedServices(model.SupportedServices),
            IsActive = true,
            IsFeatured = model.IsFeatured,
            CreatedAt = DateTime.UtcNow
        };

        AddEventTypes(venue, model.EventTypes);

        // Add amenities if provided
        var amenityIds = await GetExistingAmenityIdsAsync(model.AmenityIds);
        if (amenityIds.Any())
        {
            foreach (var amenityId in amenityIds)
            {
                venue.Amenities.Add(new VenueAmenity
                {
                    AmenityId = amenityId,
                    IsIncluded = true
                });
            }
        }

        await _venueRepository.AddAsync(venue);
        await _unitOfWork.SaveChangesAsync();

        var createdVenue = await _venueRepository.GetVenueWithDetailsAsync(venue.Id);
        return createdVenue == null ? null : MapToVenueDto(createdVenue);
    }

    public async Task<VenueDto?> UpdateVenueAsync(int id, UpdateVenueDto model)
    {
        var venue = await _venueRepository.GetVenueWithDetailsAsync(id);
        if (venue == null)
            return null;

        venue.Name = model.Name;
        venue.Description = model.Description;
        venue.Address = model.Address;
        venue.City = model.City;
        venue.Province = model.Province;
        venue.PostalCode = model.PostalCode;
        venue.Latitude = model.Latitude;
        venue.Longitude = model.Longitude;
        venue.Capacity = model.Capacity;
        venue.BasePricePerDay = model.BasePricePerDay;
        venue.IsActive = model.IsActive;
        venue.IsFeatured = model.IsFeatured;
        if (model.ImageUrl != null)
            venue.ImageUrl = model.ImageUrl;
        if (model.ThumbnailUrl != null)
            venue.ThumbnailUrl = model.ThumbnailUrl;
        venue.CustomAmenities = NormalizeCustomAmenities(model.CustomAmenities);
        venue.SupportedServices = NormalizeSupportedServices(model.SupportedServices);
        venue.UpdatedAt = DateTime.UtcNow;
        venue.Amenities.Clear();
        var amenityIds = await GetExistingAmenityIdsAsync(model.AmenityIds);
        foreach (var amenityId in amenityIds)
            venue.Amenities.Add(new VenueAmenity { AmenityId = amenityId, IsIncluded = true });
        venue.EventTypes.Clear();
        AddEventTypes(venue, model.EventTypes);

        _venueRepository.Update(venue);
        await _unitOfWork.SaveChangesAsync();

        return MapToVenueDto(venue);
    }

    public async Task<bool> DeleteVenueAsync(int id)
    {
        var result = await SetVenueVisibilityAsync(id, false);
        return result == true;
    }

    public async Task<bool?> SetVenueVisibilityAsync(int id, bool isActive)
    {
        var venue = await _venueRepository.GetByIdAsync(id);
        if (venue == null)
            return null;

        venue.IsActive = isActive;
        venue.UpdatedAt = DateTime.UtcNow;
        _venueRepository.Update(venue);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<List<VenuePhotoDto>?> AddPhotosAsync(int venueId, IEnumerable<(string FileName, string ContentType, byte[] Content)> files, int? primaryPhotoIndex = null)
    {
        var venue = await _venueRepository.GetVenueWithDetailsAsync(venueId);
        if (venue == null)
            return null;

        var nextOrder = venue.Photos.Any() ? venue.Photos.Max(photo => photo.DisplayOrder) + 1 : 0;
        var uploadedPhotos = files.Select((file, index) => new VenuePhoto
        {
            VenueId = venueId,
            Url = $"/api/venues/{venueId}/photos/pending/content",
            FileName = Path.GetFileName(file.FileName),
            ContentType = file.ContentType,
            Content = file.Content,
            DisplayOrder = nextOrder + index,
            IsPrimary = primaryPhotoIndex == index
        }).ToList();

        if (uploadedPhotos.Count == 0)
            return venue.Photos.OrderBy(photo => photo.DisplayOrder).Select(MapToPhotoDto).ToList();

        if (primaryPhotoIndex.HasValue)
            venue.Photos.Where(photo => photo.IsPrimary).ToList().ForEach(photo => photo.IsPrimary = false);

        foreach (var photo in uploadedPhotos)
            venue.Photos.Add(photo);

        await _unitOfWork.SaveChangesAsync();

        foreach (var photo in uploadedPhotos)
            photo.Url = $"/api/venues/{venueId}/photos/{photo.Id}/content";

        var primaryPhoto = venue.Photos.FirstOrDefault(photo => photo.IsPrimary) ?? venue.Photos.OrderBy(photo => photo.DisplayOrder).First();
        venue.ImageUrl = primaryPhoto.Url;
        venue.ThumbnailUrl = primaryPhoto.Url;
        venue.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return venue.Photos.OrderBy(photo => photo.DisplayOrder).Select(MapToPhotoDto).ToList();
    }

    public async Task<bool> DeletePhotoAsync(int venueId, int photoId)
    {
        var venue = await _venueRepository.GetVenueWithDetailsAsync(venueId);
        var photo = venue?.Photos.FirstOrDefault(item => item.Id == photoId);
        if (photo == null)
            return false;

        venue!.Photos.Remove(photo);
        if (photo.IsPrimary)
        {
            var replacement = venue.Photos.OrderBy(item => item.DisplayOrder).FirstOrDefault();
            if (replacement != null)
            {
                replacement.IsPrimary = true;
                venue.ImageUrl = replacement.Url;
                venue.ThumbnailUrl = replacement.Url;
            }
            else
            {
                venue.ImageUrl = null;
                venue.ThumbnailUrl = null;
            }
        }
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetPrimaryPhotoAsync(int venueId, int photoId)
    {
        var venue = await _venueRepository.GetVenueWithDetailsAsync(venueId);
        var photo = venue?.Photos.FirstOrDefault(item => item.Id == photoId);
        if (photo == null)
            return false;

        foreach (var item in venue!.Photos)
            item.IsPrimary = item.Id == photoId;
        venue.ImageUrl = photo.Url;
        venue.ThumbnailUrl = photo.Url;
        venue.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public Task<(byte[] Content, string ContentType, string FileName)?> GetPhotoContentAsync(int venueId, int photoId)
    {
        return _venueRepository.GetPhotoContentAsync(venueId, photoId);
    }

    private static VenuePhotoDto MapToPhotoDto(VenuePhoto photo) => new()
    {
        Id = photo.Id,
        Url = photo.Url,
        Caption = photo.Caption,
        DisplayOrder = photo.DisplayOrder,
        IsPrimary = photo.IsPrimary
    };

    private static List<string> NormalizeCustomAmenities(IEnumerable<string>? amenities)
    {
        return (amenities ?? Enumerable.Empty<string>())
            .Select(amenity => amenity.Trim())
            .Where(amenity => !string.IsNullOrWhiteSpace(amenity))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(50)
            .ToList();
    }

    private static List<string> NormalizeSupportedServices(IEnumerable<string>? services)
    {
        return (services ?? Enumerable.Empty<string>())
            .Select(service => service.Trim())
            .Where(service => !string.IsNullOrWhiteSpace(service))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(30)
            .ToList();
    }

    private async Task<List<int>> GetExistingAmenityIdsAsync(IEnumerable<int>? amenityIds)
    {
        var requestedIds = (amenityIds ?? Enumerable.Empty<int>()).Distinct().ToList();
        if (requestedIds.Count == 0)
            return new List<int>();

        var amenityRepository = _unitOfWork.Repository<Amenity>();
        var existing = await amenityRepository.FindAsync(amenity => requestedIds.Contains(amenity.Id));
        return existing.Select(amenity => amenity.Id).Distinct().ToList();
    }

    private static void AddEventTypes(Venue venue, IEnumerable<EventType>? eventTypes)
    {
        foreach (var eventType in eventTypes?.Distinct() ?? Enumerable.Empty<EventType>())
        {
            venue.EventTypes.Add(new VenueEventType { EventType = eventType });
        }
    }

    private static List<AvailabilityDto> BuildAvailability(Venue venue)
    {
        var availability = venue.Availabilities?.OrderBy(item => item.Date).Select(item => new AvailabilityDto
        {
            Date = item.Date,
            IsAvailable = item.IsAvailable,
            Notes = item.Notes
        }).ToList() ?? new List<AvailabilityDto>();

        var bookedDates = (venue.Bookings ?? Enumerable.Empty<Booking>())
            .Where(booking => booking.Status != BookingStatus.Cancelled && booking.Status != BookingStatus.Rejected && booking.Status != BookingStatus.QuoteRejected)
            .SelectMany(booking => Enumerable.Range(0, (booking.EndDate.Date - booking.StartDate.Date).Days + 1)
                .Select(offset => booking.StartDate.Date.AddDays(offset)))
            .ToHashSet();

        foreach (var date in bookedDates)
        {
            var existing = availability.FirstOrDefault(item => item.Date.Date == date);
            if (existing == null)
                availability.Add(new AvailabilityDto { Date = date, IsAvailable = false, Notes = "Booked" });
            else
            {
                existing.IsAvailable = false;
                existing.Notes = "Booked";
            }
        }

        return availability.OrderBy(item => item.Date).ToList();
    }

    private static VenueDto MapToVenueDto(Venue venue)
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
            IsFeatured = venue.IsFeatured,
            CreatedAt = venue.CreatedAt,
            CustomAmenities = venue.CustomAmenities ?? new List<string>(),
            SupportedServices = venue.SupportedServices ?? new List<string>(),
            Amenities = venue.Amenities?
                .Select(va => va.Amenity == null
                    ? null
                    : new AmenityDto
                    {
                        Id = va.Amenity.Id,
                        Name = va.Amenity.Name,
                        Description = va.Amenity.Description,
                        Icon = va.Amenity.Icon
                    })
                .Where(amenity => amenity != null)
                .Select(amenity => amenity!)
                .ToList() ?? new List<AmenityDto>(),
            Photos = venue.Photos?.Select(p => new VenuePhotoDto
            {
                Id = p.Id,
                Url = p.Url,
                Caption = p.Caption,
                DisplayOrder = p.DisplayOrder,
                IsPrimary = p.IsPrimary
            }).OrderBy(p => p.DisplayOrder).ToList() ?? new List<VenuePhotoDto>(),
            EventTypes = venue.EventTypes?.Select(et => et.EventType).OrderBy(et => et).ToList() ?? new List<EventType>(),
            Availabilities = BuildAvailability(venue)
        };
    }
}