using Microsoft.EntityFrameworkCore;
using Xunit;
using PremierVenue.Domain.Entities;
using PremierVenue.Infrastructure.Data;

namespace PremierVenue.Tests;

public class DatabaseTests
{
    [Fact]
    public async Task VenueSchema_PersistsFeaturedFlag()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using (var context = new AppDbContext(options))
        {
            context.Venues.Add(new Venue
            {
                Name = "Test Venue",
                City = "Cape Town",
                Province = "Western Cape",
                IsFeatured = true
            });
            await context.SaveChangesAsync();
        }

        await using (var context = new AppDbContext(options))
        {
            var venue = await context.Venues.SingleAsync();

            Assert.True(venue.IsFeatured);
            Assert.Equal("Test Venue", venue.Name);
        }
    }
}
