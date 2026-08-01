using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PremierVenue.Core.DTOs;
using PremierVenue.Core.Services;
using PremierVenue.Domain.Entities;
using PremierVenue.Domain.Enums;
using PremierVenue.Domain.Interfaces;

namespace PremierVenue.API.Controllers;

[ApiController]
[Route("api/documents")]
[Authorize]
// Manages PDF documents linked to bookings, including upload and download
public class DocumentsController : ControllerBase
{
    private const long MaxFileSize = 10 * 1024 * 1024;
    private readonly IDocumentService _documentService;
    private readonly IRepository<Booking> _bookingRepository;
    private readonly IWebHostEnvironment _environment;

    public DocumentsController(
        IDocumentService documentService,
        IRepository<Booking> bookingRepository,
        IWebHostEnvironment environment)
    {
        _documentService = documentService;
        _bookingRepository = bookingRepository;
        _environment = environment;
    }

    // Lists the documents attached to a booking
    [HttpGet("booking/{bookingId:int}")]
    public async Task<ActionResult<List<BookingDocumentDto>>> GetBookingDocuments(int bookingId)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId);
        if (booking == null || !CanAccessBooking(booking))
            return NotFound();

        return Ok(await _documentService.GetBookingDocumentsAsync(bookingId));
    }

    // Uploads a PDF document for a booking
    [HttpPost("upload")]
    [RequestSizeLimit(MaxFileSize)]
    public async Task<ActionResult<BookingDocumentDto>> Upload([FromForm] UploadDocumentDto model, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("A PDF file is required.");
        if (file.Length > MaxFileSize)
            return BadRequest("Files must be 10 MB or smaller.");
        if (!string.Equals(Path.GetExtension(file.FileName), ".pdf", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only PDF files are accepted.");

        var booking = await _bookingRepository.GetByIdAsync(model.BookingId);
        if (booking == null || !CanAccessBooking(booking))
            return NotFound();

        if (booking.Status is BookingStatus.Cancelled or BookingStatus.Rejected or BookingStatus.QuoteRejected)
            return Conflict("Documents can no longer be uploaded once a booking is cancelled or rejected.");

        var isStaff = IsStaff();
        if (!isStaff && !string.Equals(model.DocumentType, nameof(DocumentType.ProofOfPayment), StringComparison.OrdinalIgnoreCase))
            return Forbid();
        if (isStaff && string.Equals(model.DocumentType, nameof(DocumentType.ProofOfPayment), StringComparison.OrdinalIgnoreCase) == false &&
            !new[] { nameof(DocumentType.Quote), nameof(DocumentType.Invoice), nameof(DocumentType.Contract), nameof(DocumentType.Receipt), nameof(DocumentType.Insurance), nameof(DocumentType.Other) }
                .Contains(model.DocumentType, StringComparer.OrdinalIgnoreCase))
            return BadRequest("Invalid document type.");

        var relativeDirectory = Path.Combine("uploads", "bookings", booking.Id.ToString());
        var directory = Path.Combine(_environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot"), relativeDirectory);
        Directory.CreateDirectory(directory);
        var storedName = $"{Guid.NewGuid():N}.pdf";
        var storedPath = Path.Combine(directory, storedName);
        await using (var stream = System.IO.File.Create(storedPath))
        {
            await file.CopyToAsync(stream);
        }

        var userId = GetCurrentUserId();
        var document = await _documentService.UploadDocumentAsync(
            model,
            $"/{relativeDirectory.Replace('\\', '/')}/{storedName}",
            file.Length,
            Path.GetFileName(file.FileName),
            userId);

        return document == null ? NotFound() : Ok(document);
    }

    // Downloads a document by its ID
    [HttpGet("{id:int}/download")]
    public async Task<IActionResult> Download(int id)
    {
        var document = await _documentService.GetDocumentByIdAsync(id);
        if (document == null)
            return NotFound();

        var booking = await _bookingRepository.GetByIdAsync(document.BookingId);
        if (booking == null || !CanAccessBooking(booking))
            return NotFound();

        var relativePath = document.Url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var fullPath = Path.GetFullPath(Path.Combine(webRoot, relativePath));
        if (!fullPath.StartsWith(Path.GetFullPath(webRoot), StringComparison.OrdinalIgnoreCase) || !System.IO.File.Exists(fullPath))
            return NotFound();

        return PhysicalFile(fullPath, "application/pdf", document.FileName);
    }

    private bool CanAccessBooking(Booking booking) => IsStaff() || booking.ClientId == GetCurrentUserId();

    private bool IsStaff() => User.IsInRole("Staff") || User.IsInRole("Admin") || User.IsInRole("SuperAdmin");

    private int GetCurrentUserId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
}
