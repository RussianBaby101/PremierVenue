using Microsoft.EntityFrameworkCore;
using PremierVenue.Domain.Enums;
using PremierVenue.Infrastructure.Data;

namespace PremierVenue.API.Services;

// Background service that automatically completes bookings once their event end date has passed
public class BookingLifecycleHostedService : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(30);
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookingLifecycleHostedService> _logger;

    public BookingLifecycleHostedService(IServiceScopeFactory scopeFactory, ILogger<BookingLifecycleHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    // Runs the lifecycle loop on a periodic timer
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RunCycleAsync(stoppingToken);

        using var timer = new PeriodicTimer(CheckInterval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunCycleAsync(stoppingToken);
        }
    }

    // Finds confirmed or paid bookings that ended before today and marks them completed
    private async Task RunCycleAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var today = DateTime.UtcNow.Date;
            var bookingsToComplete = await dbContext.Bookings
                .Where(booking =>
                    (booking.Status == BookingStatus.Confirmed ||
                     booking.Status == BookingStatus.DepositPaid ||
                     booking.Status == BookingStatus.FullyPaid) &&
                    booking.EndDate < today)
                .ToListAsync(cancellationToken);

            if (bookingsToComplete.Count == 0)
                return;

            foreach (var booking in bookingsToComplete)
            {
                booking.Status = BookingStatus.Completed;
                booking.CompletedAt ??= DateTime.UtcNow;
                booking.UpdatedAt = DateTime.UtcNow;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Auto-completed {Count} booking(s) after event end date.", bookingsToComplete.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Booking lifecycle hosted service failed during completion check.");
        }
    }
}
