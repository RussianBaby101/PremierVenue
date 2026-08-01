using PremierVenue.Core.DTOs;

namespace PremierVenue.Core.Services;

public interface IVenueService
{
    Task<VenueDto?> GetVenueByIdAsync(int id);
    Task<PagedResponseDto<VenueDto>> GetAllVenuesAsync(int page = 1, int pageSize = 10, bool includeInactive = false, string? sortBy = null);
    Task<PagedResponseDto<VenueDto>> SearchVenuesAsync(VenueSearchDto searchDto, int page = 1, int pageSize = 10);
    Task<VenueDto?> CreateVenueAsync(CreateVenueDto model);
    Task<VenueDto?> UpdateVenueAsync(int id, UpdateVenueDto model);
    Task<bool> DeleteVenueAsync(int id);
    Task<bool?> SetVenueVisibilityAsync(int id, bool isActive);
    Task<List<VenuePhotoDto>?> AddPhotosAsync(int venueId, IEnumerable<(string FileName, string ContentType, byte[] Content)> files, int? primaryPhotoIndex = null);
    Task<bool> DeletePhotoAsync(int venueId, int photoId);
    Task<bool> SetPrimaryPhotoAsync(int venueId, int photoId);
    Task<(byte[] Content, string ContentType, string FileName)?> GetPhotoContentAsync(int venueId, int photoId);
}