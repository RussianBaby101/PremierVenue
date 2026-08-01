using PremierVenue.Core.DTOs;
using PremierVenue.Domain.Entities;
using PremierVenue.Domain.Interfaces;

namespace PremierVenue.Core.Services;

public class DocumentService : IDocumentService
{
    private readonly IRepository<BookingDocument> _documentRepository;
    private readonly IRepository<Booking> _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DocumentService(
        IRepository<BookingDocument> documentRepository,
        IRepository<Booking> bookingRepository,
        IUnitOfWork unitOfWork)
    {
        _documentRepository = documentRepository;
        _bookingRepository = bookingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BookingDocumentDto?> GetDocumentByIdAsync(int id)
    {
        var document = await _documentRepository.GetByIdAsync(id);
        return document == null ? null : Map(document);
    }

    public async Task<List<BookingDocumentDto>> GetBookingDocumentsAsync(int bookingId)
    {
        var documents = await _documentRepository.FindAsync(document => document.BookingId == bookingId);
        return documents.OrderByDescending(document => document.CreatedAt).Select(Map).ToList();
    }

    public async Task<BookingDocumentDto?> UploadDocumentAsync(
        UploadDocumentDto model,
        string fileUrl,
        long fileSize,
        string fileName,
        int userId)
    {
        var booking = await _bookingRepository.GetByIdAsync(model.BookingId);
        if (booking == null)
            return null;

        if (!Enum.TryParse<DocumentType>(model.DocumentType, true, out var documentType))
            throw new ArgumentException("Invalid document type");

        var document = new BookingDocument
        {
            BookingId = model.BookingId,
            FileName = fileName,
            Url = fileUrl,
            DocumentType = documentType,
            FileSize = fileSize,
            Description = model.Description,
            CreatedAt = DateTime.UtcNow
        };

        await _documentRepository.AddAsync(document);
        await _unitOfWork.SaveChangesAsync();
        return Map(document);
    }

    public async Task<bool> DeleteDocumentAsync(int id)
    {
        var document = await _documentRepository.GetByIdAsync(id);
        if (document == null)
            return false;

        _documentRepository.Delete(document);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private static BookingDocumentDto Map(BookingDocument document) => new()
    {
        Id = document.Id,
        BookingId = document.BookingId,
        FileName = document.FileName,
        Url = document.Url,
        DocumentType = document.DocumentType.ToString(),
        FileSize = document.FileSize,
        Description = document.Description,
        CreatedAt = document.CreatedAt
    };
}
