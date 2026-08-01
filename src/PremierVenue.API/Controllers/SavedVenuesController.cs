using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PremierVenue.Core.DTOs;
using PremierVenue.Core.Services;
 
namespace PremierVenue.API.Controllers;
 
[ApiController]
[Authorize]
[Route("api/saved-venues")]
// Allows clients to save, view, and remove favorite venues
public class SavedVenuesController : ControllerBase
{
    private readonly ISavedVenueService _savedVenueService;
 
    public SavedVenuesController(ISavedVenueService savedVenueService)
    {
        _savedVenueService = savedVenueService;
    }
 
    // Returns the list of venues saved by the current user
    [HttpGet]
    public async Task<ActionResult<List<SavedVenueDto>>> GetSavedVenues()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return Unauthorized();
        return Ok(await _savedVenueService.GetSavedVenuesAsync(userId.Value));
    }
 
    // Checks whether the given venue is saved by the current user
    [HttpGet("{venueId}/exists")]
    public async Task<ActionResult<bool>> IsSaved(int venueId)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return Unauthorized();
        return Ok(await _savedVenueService.IsSavedAsync(userId.Value, venueId));
    }
 
    // Saves the given venue for the current user
    [HttpPost("{venueId}")]
    public async Task<ActionResult<SavedVenueDto>> SaveVenue(int venueId)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return Unauthorized();
 
        var savedVenue = await _savedVenueService.SaveAsync(userId.Value, venueId);
        return savedVenue == null ? NotFound("Venue not found or is inactive") : Ok(savedVenue);
    }
 
    // Removes the given venue from the current user's saved list
    [HttpDelete("{venueId}")]
    public async Task<IActionResult> RemoveVenue(int venueId)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return Unauthorized();
 
        var removed = await _savedVenueService.RemoveAsync(userId.Value, venueId);
        return removed ? NoContent() : NotFound();
    }
 
    private int? GetCurrentUserId()
    {
        var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(value, out var userId) ? userId : null;
    }
}