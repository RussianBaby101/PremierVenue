using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PremierVenue.Core.DTOs;
using PremierVenue.Core.Services;
using PremierVenue.Core.Validators;
using FluentValidation;

namespace PremierVenue.API.Controllers;

[ApiController]
[Route("api/[controller]")]
// Manages venue listings, search, creation, updates, and photo uploads
public class VenuesController : ControllerBase
{
    private readonly IVenueService _venueService;
    private readonly IValidator<CreateVenueDto> _createVenueValidator;
    private readonly IValidator<UpdateVenueDto> _updateVenueValidator;
    private readonly ILogger<VenuesController> _logger;

    public VenuesController(
        IVenueService venueService,
        IValidator<CreateVenueDto> createVenueValidator,
        IValidator<UpdateVenueDto> updateVenueValidator,
        ILogger<VenuesController> logger)
    {
        _venueService = venueService;
        _createVenueValidator = createVenueValidator;
        _updateVenueValidator = updateVenueValidator;
        _logger = logger;
    }


    // Get all venues with pagination

    [HttpGet]
    public async Task<ActionResult<PagedResponseDto<VenueDto>>> GetVenues(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool includeInactive = false,
        [FromQuery] string? sortBy = null)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var canViewInactive = User.IsInRole("Staff") || User.IsInRole("Admin");
            var result = await _venueService.GetAllVenuesAsync(page, pageSize, canViewInactive && includeInactive, sortBy);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving venues");
            return StatusCode(500, ResponseDto<VenueDto>.ErrorResponse($"An error occurred while retrieving venues: {ex.Message}"));
        }
    }


    // Get a specific venue by ID

    [HttpGet("{id}")]
    public async Task<ActionResult<ResponseDto<VenueDto>>> GetVenue(int id)
    {
        try
        {
            var venue = await _venueService.GetVenueByIdAsync(id);
            var canViewInactive = User.IsInRole("Staff") || User.IsInRole("Admin");
            if (venue == null || (!venue.IsActive && !canViewInactive))
            {
                return NotFound(ResponseDto<VenueDto>.ErrorResponse("Venue not found"));
            }

            return Ok(ResponseDto<VenueDto>.SuccessResponse(venue, "Venue retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving venue with ID {VenueId}", id);
            return StatusCode(500, ResponseDto<VenueDto>.ErrorResponse("An error occurred while retrieving the venue"));
        }
    }


    // Search venues with filters

    [HttpPost("search")]
    public async Task<ActionResult<PagedResponseDto<VenueDto>>> SearchVenues(
        [FromBody] VenueSearchDto searchDto,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var result = await _venueService.SearchVenuesAsync(searchDto, page, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching venues");
            return StatusCode(500, ResponseDto<VenueDto>.ErrorResponse("An error occurred while searching venues"));
        }
    }


    // Create a new venue (Staff only)

    [HttpPost]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<ActionResult<ResponseDto<VenueDto>>> CreateVenue([FromBody] CreateVenueDto model)
    {
        try
        {
            var validationResult = await _createVenueValidator.ValidateAsync(model);
            if (!validationResult.IsValid)
            {
                return BadRequest(ResponseDto<VenueDto>.ErrorResponse(
                    "Validation failed",
                    validationResult.Errors.Select(e => e.ErrorMessage).ToList()));
            }

            var venue = await _venueService.CreateVenueAsync(model);
            if (venue == null)
            {
                return BadRequest(ResponseDto<VenueDto>.ErrorResponse("Failed to create venue"));
            }

            return CreatedAtAction(
                nameof(GetVenue),
                new { id = venue.Id },
                ResponseDto<VenueDto>.SuccessResponse(venue, "Venue created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating venue");
            return StatusCode(500, ResponseDto<VenueDto>.ErrorResponse("An error occurred while creating the venue"));
        }
    }


    // Update an existing venue (Staff only)

    [HttpPut("{id}")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<ActionResult<ResponseDto<VenueDto>>> UpdateVenue(int id, [FromBody] UpdateVenueDto model)
    {
        try
        {
            var validationResult = await _updateVenueValidator.ValidateAsync(model);
            if (!validationResult.IsValid)
            {
                return BadRequest(ResponseDto<VenueDto>.ErrorResponse(
                    "Validation failed",
                    validationResult.Errors.Select(e => e.ErrorMessage).ToList()));
            }

            var venue = await _venueService.UpdateVenueAsync(id, model);
            if (venue == null)
            {
                return NotFound(ResponseDto<VenueDto>.ErrorResponse("Venue not found"));
            }

            return Ok(ResponseDto<VenueDto>.SuccessResponse(venue, "Venue updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating venue with ID {VenueId}", id);
            return StatusCode(500, ResponseDto<VenueDto>.ErrorResponse("An error occurred while updating the venue"));
        }
    }

    [HttpPost("{id}/photos")]
    [Authorize(Roles = "Staff,Admin")]
    [RequestSizeLimit(50_000_000)]
    public async Task<ActionResult<ResponseDto<List<VenuePhotoDto>>>> UploadPhotos(int id, [FromForm] List<IFormFile> files, [FromForm] int? primaryPhotoIndex = null)
    {
        if (files.Count == 0)
            return BadRequest(ResponseDto<List<VenuePhotoDto>>.ErrorResponse("Select at least one image."));

        var imageFiles = new List<(string FileName, string ContentType, byte[] Content)>();
        foreach (var file in files)
        {
            if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                return BadRequest(ResponseDto<List<VenuePhotoDto>>.ErrorResponse("Only image files are supported."));
            if (file.Length == 0 || file.Length > 10_000_000)
                return BadRequest(ResponseDto<List<VenuePhotoDto>>.ErrorResponse("Each image must be smaller than 10 MB."));

            await using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            imageFiles.Add((file.FileName, file.ContentType, stream.ToArray()));
        }

        var photos = await _venueService.AddPhotosAsync(id, imageFiles, primaryPhotoIndex);
        return photos == null
            ? NotFound(ResponseDto<List<VenuePhotoDto>>.ErrorResponse("Venue not found"))
            : Ok(ResponseDto<List<VenuePhotoDto>>.SuccessResponse(photos, "Venue images uploaded successfully"));
    }

    [HttpPatch("{venueId}/photos/{photoId}/primary")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<ActionResult<ResponseDto<bool>>> SetPrimaryPhoto(int venueId, int photoId)
    {
        var updated = await _venueService.SetPrimaryPhotoAsync(venueId, photoId);
        return updated ? Ok(ResponseDto<bool>.SuccessResponse(true, "Primary venue image updated")) : NotFound(ResponseDto<bool>.ErrorResponse("Image not found"));
    }

    [HttpGet("{venueId}/photos/{photoId}/content")]
    public async Task<IActionResult> GetPhotoContent(int venueId, int photoId)
    {
        var photo = await _venueService.GetPhotoContentAsync(venueId, photoId);
        return photo == null ? NotFound() : File(photo.Value.Content, photo.Value.ContentType);
    }

    [HttpDelete("{venueId}/photos/{photoId}")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<ActionResult<ResponseDto<bool>>> DeletePhoto(int venueId, int photoId)
    {
        var deleted = await _venueService.DeletePhotoAsync(venueId, photoId);
        return deleted ? Ok(ResponseDto<bool>.SuccessResponse(true, "Venue image deleted")) : NotFound(ResponseDto<bool>.ErrorResponse("Image not found"));
    }


    // Toggle venue active status (Staff only)

    [HttpPatch("{id}/toggle-status")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<ActionResult<ResponseDto<bool>>> ToggleVenueStatus(int id)
    {
        try
        {
            var venue = await _venueService.GetVenueByIdAsync(id);
            if (venue == null)
            {
                return NotFound(ResponseDto<bool>.ErrorResponse("Venue not found"));
            }

            var newStatus = !venue.IsActive;
            var result = await _venueService.SetVenueVisibilityAsync(id, newStatus);
            if (result != true)
            {
                return BadRequest(ResponseDto<bool>.ErrorResponse("Failed to update venue status"));
            }

            var message = newStatus ? "Venue activated successfully" : "Venue deactivated successfully";
            return Ok(ResponseDto<bool>.SuccessResponse(true, message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling venue status with ID {VenueId}", id);
            return StatusCode(500, ResponseDto<bool>.ErrorResponse("An error occurred while updating venue status"));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<ActionResult<ResponseDto<bool>>> HideVenue(int id)
    {
        var hidden = await _venueService.DeleteVenueAsync(id);
        return hidden
            ? Ok(ResponseDto<bool>.SuccessResponse(true, "Venue hidden successfully"))
            : NotFound(ResponseDto<bool>.ErrorResponse("Venue not found"));
    }
}