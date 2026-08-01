using PremierVenue.Core.DTOs;

namespace PremierVenue.Core.Services;

public interface ISavedVenueService
{
    Task<List<SavedVenueDto>> GetSavedVenuesAsync(int userId);
    Task<SavedVenueDto?> SaveAsync(int userId, int venueId);
    Task<bool> RemoveAsync(int userId, int venueId);
    Task<bool> IsSavedAsync(int userId, int venueId);
}
