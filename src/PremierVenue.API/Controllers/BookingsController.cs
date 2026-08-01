using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PremierVenue.Core.DTOs;
using PremierVenue.Core.Services;
using PremierVenue.Core.Validators;
using PremierVenue.Domain.Interfaces;
using FluentValidation;

namespace PremierVenue.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
// Manages booking requests, quotes, status updates, and client/staff booking queries
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly IUserRepository _userRepository;
    private readonly IValidator<CreateBookingDto> _createBookingValidator;
    private readonly IValidator<UpdateBookingDto> _updateBookingValidator;
    private readonly IValidator<BookingStatusUpdateDto> _statusUpdateValidator;
    private readonly IValidator<BookingQuoteDto> _quoteValidator;
    private readonly ILogger<BookingsController> _logger;

    public BookingsController(
        IBookingService bookingService,
        IUserRepository userRepository,
        IValidator<CreateBookingDto> createBookingValidator,
        IValidator<UpdateBookingDto> updateBookingValidator,
        IValidator<BookingStatusUpdateDto> statusUpdateValidator,
        IValidator<BookingQuoteDto> quoteValidator,
        ILogger<BookingsController> logger)
    {
        _bookingService = bookingService;
        _userRepository = userRepository;
        _createBookingValidator = createBookingValidator;
        _updateBookingValidator = updateBookingValidator;
        _statusUpdateValidator = statusUpdateValidator;
        _quoteValidator = quoteValidator;
        _logger = logger;
    }


    // Get all bookings (Staff only)

    [HttpGet]
    public async Task<ActionResult<PagedResponseDto<BookingDto>>> GetAllBookings(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var result = await _bookingService.GetAllBookingsAsync(page, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving bookings");
            return StatusCode(500, ResponseDto<BookingDto>.ErrorResponse("An error occurred while retrieving bookings"));
        }
    }


    // Get pending booking requests (Staff only)

    [HttpGet("pending")]
    public async Task<ActionResult<PagedResponseDto<BookingDto>>> GetPendingBookings(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var result = await _bookingService.GetPendingBookingsAsync(page, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending bookings");
            return StatusCode(500, ResponseDto<BookingDto>.ErrorResponse("An error occurred while retrieving pending bookings"));
        }
    }


    // Get bookings for the authenticated client

    [HttpGet("my")]
    public async Task<ActionResult<PagedResponseDto<BookingDto>>> GetMyBookings(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var clientId = GetCurrentUserId();
            if (clientId == null)
            {
                return Unauthorized(ResponseDto<BookingDto>.ErrorResponse("User not authenticated"));
            }

            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var result = await _bookingService.GetClientBookingsAsync(clientId.Value, page, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving my bookings");
            return StatusCode(500, ResponseDto<BookingDto>.ErrorResponse("An error occurred while retrieving bookings"));
        }
    }


    // Get bookings for a specific client (Staff only)

    [HttpGet("client/{clientId}")]
    public async Task<ActionResult<PagedResponseDto<BookingDto>>> GetClientBookings(
        int clientId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var result = await _bookingService.GetClientBookingsAsync(clientId, page, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving bookings for client {ClientId}", clientId);
            return StatusCode(500, ResponseDto<BookingDto>.ErrorResponse("An error occurred while retrieving bookings"));
        }
    }


    // Get a specific booking by ID

    [HttpGet("{id}")]
    public async Task<ActionResult<ResponseDto<BookingDto>>> GetBooking(int id)
    {
        try
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null)
            {
                return NotFound(ResponseDto<BookingDto>.ErrorResponse("Booking not found"));
            }

            return Ok(ResponseDto<BookingDto>.SuccessResponse(booking, "Booking retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving booking with ID {BookingId}", id);
            return StatusCode(500, ResponseDto<BookingDto>.ErrorResponse("An error occurred while retrieving the booking"));
        }
    }


    // Get a booking by reference number

    [HttpGet("reference/{referenceNumber}")]
    public async Task<ActionResult<ResponseDto<BookingDto>>> GetBookingByReference(string referenceNumber)
    {
        try
        {
            var booking = await _bookingService.GetBookingByReferenceNumberAsync(referenceNumber);
            if (booking == null)
            {
                return NotFound(ResponseDto<BookingDto>.ErrorResponse("Booking not found"));
            }

            return Ok(ResponseDto<BookingDto>.SuccessResponse(booking, "Booking retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving booking by reference {ReferenceNumber}", referenceNumber);
            return StatusCode(500, ResponseDto<BookingDto>.ErrorResponse("An error occurred while retrieving the booking"));
        }
    }


    // Create a new booking request (Clients)

    [HttpPost]
    public async Task<ActionResult<ResponseDto<BookingDto>>> CreateBooking([FromBody] CreateBookingDto model)
    {
        try
        {
            var validationResult = await _createBookingValidator.ValidateAsync(model);
            if (!validationResult.IsValid)
            {
                return BadRequest(ResponseDto<BookingDto>.ErrorResponse(
                    "Validation failed",
                    validationResult.Errors.Select(e => e.ErrorMessage).ToList()));
            }

            // For demo: use authenticated user ID if available, otherwise default to 1
            var clientId = GetCurrentUserId() ?? 1;

            var user = await _userRepository.GetByIdAsync(clientId);
            if (user == null || !user.IsActive)
            {
                return BadRequest(ResponseDto<BookingDto>.ErrorResponse("Your account is not active. Please contact staff."));
            }

            var booking = await _bookingService.CreateBookingAsync(model, clientId);
            if (booking == null)
            {
                return BadRequest(ResponseDto<BookingDto>.ErrorResponse("Failed to create booking request"));
            }

            return CreatedAtAction(
                nameof(GetBooking),
                new { id = booking.Id },
                ResponseDto<BookingDto>.SuccessResponse(booking, "Booking request submitted successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ResponseDto<BookingDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking");
            return StatusCode(500, ResponseDto<BookingDto>.ErrorResponse("An error occurred while creating the booking"));
        }
    }


    // Update an existing booking (Staff only)

    [HttpPut("{id}")]
    public async Task<ActionResult<ResponseDto<BookingDto>>> UpdateBooking(int id, [FromBody] UpdateBookingDto model)
    {
        try
        {
            var validationResult = await _updateBookingValidator.ValidateAsync(model);
            if (!validationResult.IsValid)
            {
                return BadRequest(ResponseDto<BookingDto>.ErrorResponse(
                    "Validation failed",
                    validationResult.Errors.Select(e => e.ErrorMessage).ToList()));
            }

            var booking = await _bookingService.UpdateBookingAsync(id, model);
            if (booking == null)
            {
                return NotFound(ResponseDto<BookingDto>.ErrorResponse("Booking not found"));
            }

            return Ok(ResponseDto<BookingDto>.SuccessResponse(booking, "Booking updated successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ResponseDto<BookingDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating booking with ID {BookingId}", id);
            return StatusCode(500, ResponseDto<BookingDto>.ErrorResponse("An error occurred while updating the booking"));
        }
    }


    // Update booking status (Staff only)

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<ResponseDto<BookingDto>>> UpdateBookingStatus(int id, [FromBody] BookingStatusUpdateDto model)
    {
        try
        {
            var validationResult = await _statusUpdateValidator.ValidateAsync(model);
            if (!validationResult.IsValid)
            {
                return BadRequest(ResponseDto<BookingDto>.ErrorResponse(
                    "Validation failed",
                    validationResult.Errors.Select(e => e.ErrorMessage).ToList()));
            }

            var booking = await _bookingService.UpdateBookingStatusAsync(id, model);
            if (booking == null)
            {
                return NotFound(ResponseDto<BookingDto>.ErrorResponse("Booking not found"));
            }

            return Ok(ResponseDto<BookingDto>.SuccessResponse(booking, "Booking status updated successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ResponseDto<BookingDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating booking status for ID {BookingId}", id);
            return StatusCode(500, ResponseDto<BookingDto>.ErrorResponse("An error occurred while updating the booking status"));
        }
    }


    // Send a quote/proposal to the client (Staff only)

    [HttpPost("quote")]
    [Authorize(Roles = "Staff,Admin,SuperAdmin")]
    public async Task<ActionResult<ResponseDto<BookingDto>>> SendQuote([FromBody] BookingQuoteDto model)
    {
        try
        {
            var validationResult = await _quoteValidator.ValidateAsync(model);
            if (!validationResult.IsValid)
            {
                return BadRequest(ResponseDto<BookingDto>.ErrorResponse(
                    "Validation failed",
                    validationResult.Errors.Select(e => e.ErrorMessage).ToList()));
            }

            var booking = await _bookingService.SendQuoteAsync(model);
            if (booking == null)
            {
                return NotFound(ResponseDto<BookingDto>.ErrorResponse("Booking not found"));
            }

            return Ok(ResponseDto<BookingDto>.SuccessResponse(booking, "Quote sent successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ResponseDto<BookingDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending quote for booking {BookingId}", model.BookingId);
            return StatusCode(500, ResponseDto<BookingDto>.ErrorResponse("An error occurred while sending the quote"));
        }
    }

    [HttpPost("{id}/quote-decision")]
    [Authorize]
    public async Task<ActionResult<ResponseDto<BookingDto>>> DecideQuote(int id, [FromBody] QuoteDecisionDto model)
    {
        var clientId = GetCurrentUserId();
        if (!clientId.HasValue)
            return Unauthorized(ResponseDto<BookingDto>.ErrorResponse("User not authenticated"));

        var booking = await _bookingService.DecideQuoteAsync(id, model, clientId.Value);
        return booking == null
            ? NotFound(ResponseDto<BookingDto>.ErrorResponse("Quote not found or no longer available"))
            : Ok(ResponseDto<BookingDto>.SuccessResponse(booking, model.Accepted ? "Quote accepted" : "Quote rejected"));
    }


    // Delete a booking (Staff only)

    [HttpDelete("{id}")]
    public async Task<ActionResult<ResponseDto<bool>>> DeleteBooking(int id)
    {
        try
        {
            var result = await _bookingService.DeleteBookingAsync(id);
            if (!result)
            {
                return NotFound(ResponseDto<bool>.ErrorResponse("Booking not found"));
            }

            return Ok(ResponseDto<bool>.SuccessResponse(true, "Booking deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting booking with ID {BookingId}", id);
            return StatusCode(500, ResponseDto<bool>.ErrorResponse("An error occurred while deleting the booking"));
        }
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
            return userId;

        return null;
    }
}