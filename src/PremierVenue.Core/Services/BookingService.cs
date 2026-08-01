using PremierVenue.Core.DTOs;
using PremierVenue.Domain.Entities;
using PremierVenue.Domain.Enums;
using PremierVenue.Domain.Interfaces;

namespace PremierVenue.Core.Services;

public class BookingService : IBookingService
{
    private static readonly BookingStatus[] TerminalStatuses =
    {
        BookingStatus.Cancelled,
        BookingStatus.Rejected,
        BookingStatus.QuoteRejected
    };

    private readonly IBookingRepository _bookingRepository;
    private readonly IVenueRepository _venueRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BookingService(
        IBookingRepository bookingRepository,
        IVenueRepository venueRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _bookingRepository = bookingRepository;
        _venueRepository = venueRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BookingDto?> GetBookingByIdAsync(int id)
    {
        await AutoCompleteElapsedBookingsAsync();

        var booking = await _bookingRepository.GetByIdAsync(id);
        if (booking == null)
            return null;

        return await MapToBookingDtoAsync(booking);
    }

    public async Task<BookingDto?> GetBookingByReferenceNumberAsync(string referenceNumber)
    {
        await AutoCompleteElapsedBookingsAsync();

        var booking = await _bookingRepository.GetByReferenceNumberAsync(referenceNumber);
        if (booking == null)
            return null;

        return await MapToBookingDtoAsync(booking);
    }

    public async Task<PagedResponseDto<BookingDto>> GetAllBookingsAsync(int page = 1, int pageSize = 10)
    {
        await AutoCompleteElapsedBookingsAsync();

        var bookings = (await _bookingRepository.GetAllAsync()).OrderByDescending(b => b.CreatedAt).ToList();
        return await PaginateAsync(bookings, page, pageSize);
    }

    public async Task<PagedResponseDto<BookingDto>> GetClientBookingsAsync(int clientId, int page = 1, int pageSize = 10)
    {
        await AutoCompleteElapsedBookingsAsync();

        var bookings = (await _bookingRepository.GetClientBookingsAsync(clientId)).ToList();
        return await PaginateAsync(bookings, page, pageSize);
    }

    public async Task<PagedResponseDto<BookingDto>> GetPendingBookingsAsync(int page = 1, int pageSize = 10)
    {
        await AutoCompleteElapsedBookingsAsync();

        var bookings = (await _bookingRepository.GetPendingBookingsAsync()).ToList();
        return await PaginateAsync(bookings, page, pageSize);
    }

    public async Task<BookingDto?> CreateBookingAsync(CreateBookingDto model, int clientId)
    {
        var venue = await _venueRepository.GetByIdAsync(model.VenueId);
        if (venue == null)
            return null;

        var user = await _userRepository.GetByIdAsync(clientId);
        if (user == null)
            return null;

        if (!Enum.TryParse<EventType>(model.EventType, out var eventType))
            return null;

        if (await HasDateConflictAsync(model.VenueId, model.StartDate, model.EndDate))
            throw new InvalidOperationException("The selected venue is already booked for one or more of those dates.");

        var booking = new Booking
        {
            ReferenceNumber = GenerateReferenceNumber(),
            ClientId = clientId,
            VenueId = model.VenueId,
            EventType = eventType,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            ExpectedGuests = model.ExpectedGuests,
            SpecialRequirements = model.SpecialRequirements ?? string.Empty,
            CateringRequested = model.CateringRequested,
            StaffingSecurityRequested = model.StaffingSecurityRequested,
            SetupCleanupRequested = model.SetupCleanupRequested,
            AdditionalServices = model.AdditionalServices ?? string.Empty,
            FinalQuote = 0,
            DepositAmount = 0,
            BalanceAmount = 0,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _bookingRepository.AddAsync(booking);
        await _unitOfWork.SaveChangesAsync();

        return await GetBookingByIdAsync(booking.Id);
    }

    public async Task<BookingDto?> UpdateBookingAsync(int id, UpdateBookingDto model)
    {
        var booking = await _bookingRepository.GetByIdAsync(id);
        if (booking == null)
            return null;

        if (IsTerminalStatus(booking.Status))
            throw new InvalidOperationException("Cancelled or rejected bookings cannot be modified.");

        if (!Enum.TryParse<EventType>(model.EventType, out var eventType))
            return null;

        if (await HasDateConflictAsync(booking.VenueId, model.StartDate, model.EndDate, booking.Id))
            throw new InvalidOperationException("The selected venue is already booked for one or more of those dates.");

        booking.EventType = eventType;
        booking.StartDate = model.StartDate;
        booking.EndDate = model.EndDate;
        booking.ExpectedGuests = model.ExpectedGuests;
        booking.SpecialRequirements = model.SpecialRequirements;
        booking.CateringRequested = model.CateringRequested;
        booking.StaffingSecurityRequested = model.StaffingSecurityRequested;
        booking.SetupCleanupRequested = model.SetupCleanupRequested;
        booking.AdditionalServices = model.AdditionalServices;
        booking.FinalQuote = model.FinalQuote;
        booking.DepositAmount = model.DepositAmount;
        booking.QuoteExpiresAt = model.QuoteExpiresAt;
        booking.CancellationPolicy = model.CancellationPolicy;
        booking.CancellationPolicyCode = model.CancellationPolicyCode;
        booking.BalanceAmount = Math.Max(0, model.FinalQuote - model.DepositAmount);
        booking.InternalNotes = model.InternalNotes;
        booking.UpdatedAt = DateTime.UtcNow;

        _bookingRepository.Update(booking);
        await _unitOfWork.SaveChangesAsync();

        return await MapToBookingDtoAsync(booking);
    }

    public async Task<BookingDto?> UpdateBookingStatusAsync(int id, BookingStatusUpdateDto model)
    {
        var booking = await _bookingRepository.GetByIdAsync(id);
        if (booking == null)
            return null;

        if (IsTerminalStatus(booking.Status))
            throw new InvalidOperationException("This booking is in a terminal status and can no longer be changed.");

        if (!Enum.TryParse<BookingStatus>(model.Status, out var status))
            return null;

        if (status == BookingStatus.Completed)
            throw new InvalidOperationException("Completed is set automatically after the event has ended.");

        booking.Status = status;
        booking.UpdatedAt = DateTime.UtcNow;

        if (status == BookingStatus.Confirmed && booking.ConfirmedAt == null)
        {
            booking.ConfirmedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<TaskItem>().AddAsync(new TaskItem
            {
                BookingId = booking.Id,
                Title = "Send Invoice",
                Description = $"Booking {booking.ReferenceNumber} has been confirmed. Send the invoice to the client via email.",
                Status = PremierVenue.Domain.Entities.TaskStatus.NotStarted,
                Priority = Priority.High,
                DueDate = DateTime.UtcNow.AddDays(1)
            });
        }

        if (status == BookingStatus.DepositPaid || status == BookingStatus.FullyPaid)
            await RecordPaymentForStatusAsync(booking, status);

        var shouldApplyCancellationRefund = status == BookingStatus.Cancelled &&
            (booking.CancelledAt == null ||
             (booking.RefundAmount == 0m && (string.IsNullOrWhiteSpace(booking.RefundStatus) || booking.RefundStatus == "NotApplicable")));

        if (shouldApplyCancellationRefund)
            await ApplyCancellationRefundAsync(booking);

        if (status == BookingStatus.Completed && booking.CompletedAt == null)
            booking.CompletedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(model.Notes))
            booking.InternalNotes = model.Notes;

        _bookingRepository.Update(booking);
        await _unitOfWork.SaveChangesAsync();

        return await MapToBookingDtoAsync(booking);
    }

    public async Task<BookingDto?> SendQuoteAsync(BookingQuoteDto model)
    {
        var booking = await _bookingRepository.GetByIdAsync(model.BookingId);
        if (booking == null)
            return null;

        if (IsTerminalStatus(booking.Status))
            throw new InvalidOperationException("Cancelled or rejected bookings cannot receive new quotes.");

        booking.FinalQuote = model.FinalQuote;
        booking.DepositAmount = model.DepositAmount;
        booking.QuoteExpiresAt = model.QuoteExpiresAt;
        booking.CancellationPolicy = model.CancellationPolicy;
        booking.CancellationPolicyCode = model.CancellationPolicyCode;
        booking.BalanceAmount = Math.Max(0, model.FinalQuote - model.DepositAmount);
        booking.Status = BookingStatus.Quoted;
        booking.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(model.Notes))
            booking.InternalNotes = model.Notes;

        _bookingRepository.Update(booking);
        await _unitOfWork.SaveChangesAsync();

        return await MapToBookingDtoAsync(booking);
    }

    public async Task<BookingDto?> DecideQuoteAsync(int bookingId, QuoteDecisionDto model, int clientId)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId);
        if (booking == null || booking.ClientId != clientId || booking.Status != BookingStatus.Quoted)
            return null;

        booking.Status = model.Accepted ? BookingStatus.QuoteAccepted : BookingStatus.QuoteRejected;
        booking.InternalNotes = string.IsNullOrWhiteSpace(model.Notes) ? booking.InternalNotes : model.Notes;
        booking.UpdatedAt = DateTime.UtcNow;
        _bookingRepository.Update(booking);
        await _unitOfWork.SaveChangesAsync();
        return await MapToBookingDtoAsync(booking);
    }

    public async Task<bool> DeleteBookingAsync(int id)
    {
        var booking = await _bookingRepository.GetByIdAsync(id);
        if (booking == null)
            return false;

        _bookingRepository.Delete(booking);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private async Task RecordPaymentForStatusAsync(Booking booking, BookingStatus status)
    {
        var paymentRepository = _unitOfWork.Repository<Payment>();
        var payments = (await paymentRepository.FindAsync(payment => payment.BookingId == booking.Id)).ToList();
        var completedNonRefunds = payments.Where(payment => payment.Status == PaymentStatus.Completed && payment.PaymentType != PaymentType.Refund).ToList();

        if (status == BookingStatus.DepositPaid && !completedNonRefunds.Any(payment => payment.PaymentType == PaymentType.Deposit))
        {
            await paymentRepository.AddAsync(new Payment
            {
                BookingId = booking.Id,
                Amount = booking.DepositAmount,
                PaymentType = PaymentType.Deposit,
                Status = PaymentStatus.Completed,
                TransactionReference = $"POP-{booking.ReferenceNumber}-DEPOSIT",
                ProcessedAt = DateTime.UtcNow
            });
        }
        else if (status == BookingStatus.FullyPaid && !completedNonRefunds.Any(payment => payment.PaymentType == PaymentType.FullPayment))
        {
            var paid = completedNonRefunds.Sum(payment => payment.Amount);
            var outstanding = Math.Max(0, booking.FinalQuote - paid);
            if (outstanding > 0)
            {
                await paymentRepository.AddAsync(new Payment
                {
                    BookingId = booking.Id,
                    Amount = outstanding,
                    PaymentType = PaymentType.FullPayment,
                    Status = PaymentStatus.Completed,
                    TransactionReference = $"POP-{booking.ReferenceNumber}-BALANCE",
                    ProcessedAt = DateTime.UtcNow
                });
            }
        }
    }

    private async Task ApplyCancellationRefundAsync(Booking booking)
    {
        var paymentRepository = _unitOfWork.Repository<Payment>();
        var payments = (await paymentRepository.FindAsync(payment => payment.BookingId == booking.Id)).ToList();
        var paid = payments
            .Where(payment => payment.Status == PaymentStatus.Completed && payment.PaymentType != PaymentType.Refund)
            .Sum(payment => payment.Amount);
        var refunded = payments
            .Where(payment => payment.Status == PaymentStatus.Completed && payment.PaymentType == PaymentType.Refund)
            .Sum(payment => payment.Amount);
        var refundableBase = Math.Max(0, paid - refunded);
        var cancellationPolicyCode = string.IsNullOrWhiteSpace(booking.CancellationPolicyCode) ? "Standard" : booking.CancellationPolicyCode;
        var daysUntilEvent = (booking.StartDate.Date - DateTime.UtcNow.Date).Days;
        var percentage = cancellationPolicyCode switch
        {
            "FullRefund" => 100m,
            "NoRefund" => 0m,
            _ when daysUntilEvent > 30 => 100m,
            _ when daysUntilEvent >= 15 => 50m,
            _ when daysUntilEvent >= 7 => 25m,
            _ => 0m
        };
        var refundAmount = Math.Round(refundableBase * percentage / 100m, 2, MidpointRounding.AwayFromZero);
        var feePercentage = Math.Max(0m, 100m - percentage);
        var requiredCancellationFee = Math.Round(Math.Max(0, booking.DepositAmount) * feePercentage / 100m, 2, MidpointRounding.AwayFromZero);
        var outstandingCancellationFee = Math.Round(Math.Max(0, requiredCancellationFee - paid), 2, MidpointRounding.AwayFromZero);

        booking.CancellationPolicyCode = cancellationPolicyCode;
        booking.RefundAmount = refundAmount;
        booking.RefundStatus = refundAmount > 0
            ? "Processed"
            : paid <= 0 ? "NoPaymentToRefund" : "NotDue";
        booking.CancellationFeeAmount = outstandingCancellationFee;
        booking.CancellationFeeStatus = outstandingCancellationFee > 0 ? "Due" : "NotDue";
        booking.CancellationFeeDueAt = outstandingCancellationFee > 0 ? DateTime.UtcNow.AddDays(7) : null;
        booking.CancelledAt = DateTime.UtcNow;

        if (refundAmount > 0)
        {
            await paymentRepository.AddAsync(new Payment
            {
                BookingId = booking.Id,
                Amount = refundAmount,
                PaymentType = PaymentType.Refund,
                Status = PaymentStatus.Completed,
                TransactionReference = $"REFUND-{booking.ReferenceNumber}",
                PaymentGatewayResponse = $"Cancellation refund calculated at {percentage}% under {cancellationPolicyCode} policy.",
                ProcessedAt = DateTime.UtcNow
            });
        }
    }

    private static bool IsTerminalStatus(BookingStatus status)
    {
        return TerminalStatuses.Contains(status);
    }

    private async Task AutoCompleteElapsedBookingsAsync()
    {
        var today = DateTime.UtcNow.Date;
        var bookingsToComplete = (await _bookingRepository.FindAsync(booking =>
            (booking.Status == BookingStatus.Confirmed ||
             booking.Status == BookingStatus.DepositPaid ||
             booking.Status == BookingStatus.FullyPaid) &&
            booking.EndDate < today)).ToList();

        if (bookingsToComplete.Count == 0)
            return;

        foreach (var booking in bookingsToComplete)
        {
            booking.Status = BookingStatus.Completed;
            if (booking.CompletedAt == null)
                booking.CompletedAt = DateTime.UtcNow;
            booking.UpdatedAt = DateTime.UtcNow;
            _bookingRepository.Update(booking);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<bool> HasDateConflictAsync(int venueId, DateTime startDate, DateTime endDate, int? excludedBookingId = null)
    {
        var bookings = await _bookingRepository.FindAsync(b =>
            b.VenueId == venueId &&
            b.Status != BookingStatus.Cancelled &&
            b.Status != BookingStatus.Rejected &&
            b.Status != BookingStatus.QuoteRejected &&
            (!excludedBookingId.HasValue || b.Id != excludedBookingId.Value) &&
            b.StartDate <= endDate &&
            b.EndDate >= startDate);

        return bookings.Any();
    }

    private static string GenerateReferenceNumber()
    {
        const string prefix = "PV";
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = new Random().Next(1000, 9999);
        return $"{prefix}-{timestamp}-{random}";
    }

    private async Task<BookingDto> MapToBookingDtoAsync(Booking booking)
    {
        var client = booking.Client ?? await _userRepository.GetByIdAsync(booking.ClientId);
        var venue = booking.Venue ?? await _venueRepository.GetByIdAsync(booking.VenueId);

        return new BookingDto
        {
            Id = booking.Id,
            ReferenceNumber = booking.ReferenceNumber,
            ClientId = booking.ClientId,
            ClientName = client != null ? $"{client.FirstName} {client.LastName}".Trim() : "Unknown",
            VenueId = booking.VenueId,
            VenueName = venue?.Name ?? "Unknown",
            EventType = booking.EventType.ToString(),
            StartDate = booking.StartDate,
            EndDate = booking.EndDate,
            ExpectedGuests = booking.ExpectedGuests,
            SpecialRequirements = booking.SpecialRequirements,
            CateringRequested = booking.CateringRequested,
            StaffingSecurityRequested = booking.StaffingSecurityRequested,
            SetupCleanupRequested = booking.SetupCleanupRequested,
            AdditionalServices = booking.AdditionalServices,
            FinalQuote = booking.FinalQuote,
            DepositAmount = booking.DepositAmount,
            QuoteExpiresAt = booking.QuoteExpiresAt,
            CancellationPolicy = booking.CancellationPolicy,
            CancellationPolicyCode = booking.CancellationPolicyCode,
            BalanceAmount = booking.BalanceAmount,
            RefundAmount = booking.RefundAmount,
            RefundStatus = booking.RefundStatus,
            CancellationFeeAmount = booking.CancellationFeeAmount,
            CancellationFeeStatus = booking.CancellationFeeStatus,
            CancellationFeeDueAt = booking.CancellationFeeDueAt,
            CancelledAt = booking.CancelledAt,
            Status = booking.Status.ToString(),
            InternalNotes = booking.InternalNotes,
            CreatedAt = booking.CreatedAt,
            UpdatedAt = booking.UpdatedAt,
            ConfirmedAt = booking.ConfirmedAt,
            CompletedAt = booking.CompletedAt,
            Payments = booking.Payments?.Select(p => new PaymentDto
            {
                Id = p.Id,
                BookingId = p.BookingId,
                ReferenceNumber = p.TransactionReference ?? string.Empty,
                Amount = p.Amount,
                PaymentType = p.PaymentType.ToString(),
                Status = p.Status.ToString(),
                TransactionReference = p.TransactionReference,
                CreatedAt = p.CreatedAt,
                ProcessedAt = p.ProcessedAt
            }).ToList() ?? new List<PaymentDto>(),
            Documents = booking.Documents?.Select(d => new BookingDocumentDto
            {
                Id = d.Id,
                BookingId = d.BookingId,
                FileName = d.FileName,
                Url = d.Url,
                DocumentType = d.DocumentType.ToString(),
                FileSize = d.FileSize,
                Description = d.Description,
                CreatedAt = d.CreatedAt
            }).ToList() ?? new List<BookingDocumentDto>()
        };
    }

    private async Task<PagedResponseDto<BookingDto>> PaginateAsync(IEnumerable<Booking> bookings, int page, int pageSize)
    {
        var bookingList = bookings.ToList();
        var totalCount = bookingList.Count;
        var pagedItems = bookingList
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var dtos = new List<BookingDto>();
        foreach (var booking in pagedItems)
        {
            dtos.Add(await MapToBookingDtoAsync(booking));
        }

        return PagedResponseDto<BookingDto>.Create(dtos, page, pageSize, totalCount);
    }
}