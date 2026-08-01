using PremierVenue.Domain.Entities;

namespace PremierVenue.Domain.Interfaces;

public interface ISavedVenueRepository
{
    Task<List<SavedVenue>> GetByUserIdAsync(int userId);
    Task<SavedVenue?> GetAsync(int userId, int venueId);
    Task AddAsync(SavedVenue savedVenue);
    void Delete(SavedVenue savedVenue);
}
