using Xunit;
using PremierVenue.Domain.Entities;
using PremierVenue.Domain.Enums;

namespace PremierVenue.Tests;

public class DomainTests
{
    [Fact]
    public void NewVenue_IsActiveAndNotFeaturedByDefault()
    {
        var venue = new Venue();

        Assert.True(venue.IsActive);
        Assert.False(venue.IsFeatured);
    }

    [Fact]
    public void QuoteRejected_IsDistinctFromOtherBookingStatuses()
    {
        Assert.NotEqual(BookingStatus.QuoteRejected, BookingStatus.Pending);
        Assert.NotEqual(BookingStatus.QuoteRejected, BookingStatus.Cancelled);
        Assert.Equal(10, (int)BookingStatus.QuoteRejected);
    }
}
