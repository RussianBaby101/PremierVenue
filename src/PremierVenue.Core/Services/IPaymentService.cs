using PremierVenue.Core.DTOs;

namespace PremierVenue.Core.Services;

public interface IPaymentService
{
    Task<PaymentDto?> GetPaymentByIdAsync(int id);
    Task<List<PaymentDto>> GetBookingPaymentsAsync(int bookingId);
    Task<PaymentResponseDto> InitiatePaymentAsync(CreatePaymentDto model);
    Task<PaymentDto?> ProcessPaymentCallbackAsync(string transactionReference, string status);
    Task<bool> RefundPaymentAsync(int paymentId);
}