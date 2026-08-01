using Microsoft.EntityFrameworkCore;
using PremierVenue.Domain.Entities;
using PremierVenue.Domain.Interfaces;
using PremierVenue.Infrastructure.Data;

namespace PremierVenue.Infrastructure.Repositories;

// Repository for saved venue records, including related venue details
public class SavedVenueRepository : ISavedVenueRepository
{
    private readonly AppDbContext _context;

    public SavedVenueRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<SavedVenue>> GetByUserIdAsync(int userId)
    {
        return _context.SavedVenues
            .Where(saved => saved.UserId == userId)
            .Include(saved => saved.Venue)
                .ThenInclude(venue => venue.Amenities)
                    .ThenInclude(venueAmenity => venueAmenity.Amenity)
            .Include(saved => saved.Venue)
                .ThenInclude(venue => venue.Photos)
            .Include(saved => saved.Venue)
                .ThenInclude(venue => venue.EventTypes)
            .OrderByDescending(saved => saved.CreatedAt)
            .ToListAsync();
    }

    public Task<SavedVenue?> GetAsync(int userId, int venueId)
    {
        return _context.SavedVenues.FirstOrDefaultAsync(saved => saved.UserId == userId && saved.VenueId == venueId);
    }

    public async Task AddAsync(SavedVenue savedVenue)
    {
        await _context.SavedVenues.AddAsync(savedVenue);
    }

    public void Delete(SavedVenue savedVenue)
    {
        _context.SavedVenues.Remove(savedVenue);
    }
}
