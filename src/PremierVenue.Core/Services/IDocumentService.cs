using PremierVenue.Core.DTOs;
namespace PremierVenue.Core.Services;

public interface IDocumentService
{
    Task<BookingDocumentDto?> GetDocumentByIdAsync(int id);
    Task<List<BookingDocumentDto>> GetBookingDocumentsAsync(int bookingId);
    Task<BookingDocumentDto?> UploadDocumentAsync(UploadDocumentDto model, string fileUrl, long fileSize, string fileName, int userId);
    Task<bool> DeleteDocumentAsync(int id);
}