using PremierVenue.Core.Utilities;
using PremierVenue.Domain.Entities;
using PremierVenue.Domain.Enums;

namespace PremierVenue.Infrastructure.Data;

public static class DataSeeder
{
    public static void Seed(AppDbContext context)
    {
        SeedUsers(context);

        // Check if venue data already exists
        if (context.Venues.Any())
        {
            SeedVenueEventTypes(context);
            return;
        }

        // Seed Amenities
        var amenities = new List<Amenity>
        {
            new Amenity { Name = "Parking", Description = "Free parking available", Icon = "bi-parking", IsActive = true },
            new Amenity { Name = "WiFi", Description = "High-speed internet access", Icon = "bi-wifi", IsActive = true },
            new Amenity { Name = "Catering", Description = "In-house catering services", Icon = "bi-cup", IsActive = true },
            new Amenity { Name = "AV System", Description = "Audio-visual equipment", Icon = "bi-display", IsActive = true },
            new Amenity { Name = "Security", Description = "24/7 security personnel", Icon = "bi-shield", IsActive = true },
            new Amenity { Name = "Stage", Description = "Built-in stage area", Icon = "bi-music-note", IsActive = true },
            new Amenity { Name = "Outdoor Space", Description = "Garden or outdoor area", Icon = "bi-tree", IsActive = true },
            new Amenity { Name = "Bar", Description = "Full bar service available", Icon = "bi-cup-fill", IsActive = true },
            new Amenity { Name = "Decor", Description = "Event decoration services", Icon = "bi-palette", IsActive = true },
            new Amenity { Name = "Air Conditioning", Description = "Climate control system", Icon = "bi-snow", IsActive = true }
        };

        context.Amenities.AddRange(amenities);
        context.SaveChanges();

        // Seed Venues
        var venues = new List<Venue>
        {
            new Venue
            {
                Name = "Lakeside Pavilion",
                Description = "Beautiful waterfront venue with stunning views, perfect for weddings and corporate events. Features a spacious main hall, outdoor terrace, and elegant interiors.",
                Address = "123 Waterfront Drive",
                City = "Cape Town",
                Province = "Western Cape",
                PostalCode = "8001",
                Latitude = -33.9249m,
                Longitude = 18.4241m,
                Capacity = 220,
                BasePricePerDay = 18000,
                ImageUrl = "https://images.unsplash.com/photo-1519167758481-83f550bb49b3?w=800&h=600&fit=crop",
                ThumbnailUrl = "https://images.unsplash.com/photo-1519167758481-83f550bb49b3?w=200&h=150&fit=crop",
                IsActive = true,
                SupportedServices = new List<string> { "Catering", "Staffing & security", "Setup & cleanup", "AV technician" },
                EventTypes = new List<VenueEventType>
                {
                    new VenueEventType { EventType = EventType.Wedding },
                    new VenueEventType { EventType = EventType.Corporate }
                },
                Amenities = new List<VenueAmenity>
                {
                    new VenueAmenity { AmenityId = amenities[0].Id, IsIncluded = true }, // Parking
                    new VenueAmenity { AmenityId = amenities[1].Id, IsIncluded = true }, // WiFi
                    new VenueAmenity { AmenityId = amenities[2].Id, IsIncluded = true }, // Catering
                    new VenueAmenity { AmenityId = amenities[9].Id, IsIncluded = true }  // Air Conditioning
                },
                Photos = new List<VenuePhoto>
                {
                    new VenuePhoto { Url = "https://images.unsplash.com/photo-1519167758481-83f550bb49b3?w=800&h=600&fit=crop", Caption = "Main Hall", DisplayOrder = 1, IsPrimary = true },
                    new VenuePhoto { Url = "https://images.unsplash.com/photo-1464366400600-7168b8af9bc3?w=800&h=600&fit=crop", Caption = "Outdoor Terrace", DisplayOrder = 2, IsPrimary = false },
                    new VenuePhoto { Url = "https://images.unsplash.com/photo-1429497419839-1c6946e6c5f9?w=800&h=600&fit=crop", Caption = "Reception Area", DisplayOrder = 3, IsPrimary = false }
                }
            },
            new Venue
            {
                Name = "Summit Conference Hall",
                Description = "Modern conference facility with state-of-the-art technology. Ideal for business meetings, conferences, and corporate presentations.",
                Address = "456 Business Park, Sandton",
                City = "Johannesburg",
                Province = "Gauteng",
                PostalCode = "2196",
                Latitude = -26.1077m,
                Longitude = 28.0556m,
                Capacity = 400,
                BasePricePerDay = 32000,
                ImageUrl = "https://images.unsplash.com/photo-1478146059778-26a4c107e3ae?w=800&h=600&fit=crop",
                ThumbnailUrl = "https://images.unsplash.com/photo-1478146059778-26a4c107e3ae?w=200&h=150&fit=crop",
                IsActive = true,
                SupportedServices = new List<string> { "Catering", "Staffing & security", "Setup & cleanup", "AV technician" },
                EventTypes = new List<VenueEventType>
                {
                    new VenueEventType { EventType = EventType.Corporate },
                    new VenueEventType { EventType = EventType.Conference }
                },
                Amenities = new List<VenueAmenity>
                {
                    new VenueAmenity { AmenityId = amenities[0].Id, IsIncluded = true }, // Parking
                    new VenueAmenity { AmenityId = amenities[1].Id, IsIncluded = true }, // WiFi
                    new VenueAmenity { AmenityId = amenities[3].Id, IsIncluded = true }, // AV System
                    new VenueAmenity { AmenityId = amenities[4].Id, IsIncluded = true }, // Security
                    new VenueAmenity { AmenityId = amenities[5].Id, IsIncluded = true }, // Stage
                    new VenueAmenity { AmenityId = amenities[9].Id, IsIncluded = true }  // Air Conditioning
                },
                Photos = new List<VenuePhoto>
                {
                    new VenuePhoto { Url = "https://images.unsplash.com/photo-1478146059778-26a4c107e3ae?w=800&h=600&fit=crop", Caption = "Conference Room", DisplayOrder = 1, IsPrimary = true },
                    new VenuePhoto { Url = "https://images.unsplash.com/photo-1511578314322-379afb476865?w=800&h=600&fit=crop", Caption = "Lobby Area", DisplayOrder = 2, IsPrimary = false }
                }
            },
            new Venue
            {
                Name = "Garden Terrace Venue",
                Description = "Charming outdoor venue surrounded by lush gardens. Perfect for intimate weddings, birthday parties, and social gatherings.",
                Address = "789 Garden Road, Durban North",
                City = "Durban",
                Province = "KwaZulu-Natal",
                PostalCode = "4051",
                Latitude = -29.8485m,
                Longitude = 31.0184m,
                Capacity = 140,
                BasePricePerDay = 12500,
                ImageUrl = "https://images.unsplash.com/photo-1464366400600-7168b8af9bc3?w=800&h=600&fit=crop",
                ThumbnailUrl = "https://images.unsplash.com/photo-1464366400600-7168b8af9bc3?w=200&h=150&fit=crop",
                IsActive = true,
                SupportedServices = new List<string> { "Catering", "Staffing & security", "Setup & cleanup", "Decor planning" },
                EventTypes = new List<VenueEventType>
                {
                    new VenueEventType { EventType = EventType.Wedding },
                    new VenueEventType { EventType = EventType.Birthday },
                    new VenueEventType { EventType = EventType.PrivateParty }
                },
                Amenities = new List<VenueAmenity>
                {
                    new VenueAmenity { AmenityId = amenities[0].Id, IsIncluded = true }, // Parking
                    new VenueAmenity { AmenityId = amenities[6].Id, IsIncluded = true }, // Outdoor Space
                    new VenueAmenity { AmenityId = amenities[7].Id, IsIncluded = true }, // Bar
                    new VenueAmenity { AmenityId = amenities[8].Id, IsIncluded = true }  // Decor
                },
                Photos = new List<VenuePhoto>
                {
                    new VenuePhoto { Url = "https://images.unsplash.com/photo-1464366400600-7168b8af9bc3?w=800&h=600&fit=crop", Caption = "Garden View", DisplayOrder = 1, IsPrimary = true },
                    new VenuePhoto { Url = "https://images.unsplash.com/photo-1429497419839-1c6946e6c5f9?w=800&h=600&fit=crop", Caption = "Ceremony Area", DisplayOrder = 2, IsPrimary = false }
                }
            }
        };

        context.Venues.AddRange(venues);
        context.SaveChanges();

        // Seed Availability for the next 30 days for each venue
        var startDate = DateTime.Today;
        for (int i = 0; i < 30; i++)
        {
            var date = startDate.AddDays(i);

            foreach (var venue in venues)
            {
                // Randomly set some dates as booked (20% chance)
                var isAvailable = new Random().NextDouble() > 0.2;

                context.Availabilities.Add(new Availability
                {
                    VenueId = venue.Id,
                    Date = date,
                    IsAvailable = isAvailable,
                    Notes = isAvailable ? null : "Reserved for existing booking"
                });
            }
        }

        context.SaveChanges();
    }

    private static void SeedVenueEventTypes(AppDbContext context)
    {
        if (context.VenueEventTypes.Any()) return;

        var venues = context.Venues.OrderBy(venue => venue.Id).ToList();
        if (venues.Count > 0)
        {
            context.VenueEventTypes.AddRange(new VenueEventType { VenueId = venues[0].Id, EventType = EventType.Wedding }, new VenueEventType { VenueId = venues[0].Id, EventType = EventType.Corporate });
        }
        if (venues.Count > 1)
        {
            context.VenueEventTypes.AddRange(new VenueEventType { VenueId = venues[1].Id, EventType = EventType.Corporate }, new VenueEventType { VenueId = venues[1].Id, EventType = EventType.Conference });
        }
        if (venues.Count > 2)
        {
            context.VenueEventTypes.AddRange(new VenueEventType { VenueId = venues[2].Id, EventType = EventType.Wedding }, new VenueEventType { VenueId = venues[2].Id, EventType = EventType.Birthday }, new VenueEventType { VenueId = venues[2].Id, EventType = EventType.PrivateParty });
        }

        context.SaveChanges();
    }

    private static void SeedUsers(AppDbContext context)
    {
        if (context.Users.Any())
            return;

        var users = new List<User>
        {
            new()
            {
                Email = "admin@premiervenue.com",
                UserName = "admin",
                PasswordHash = PasswordHasher.HashPassword("Password123!"),
                FirstName = "Admin",
                LastName = "User",
                PhoneNumber = "0123456789",
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Email = "staff@premiervenue.com",
                UserName = "staff",
                PasswordHash = PasswordHasher.HashPassword("Password123!"),
                FirstName = "Staff",
                LastName = "User",
                PhoneNumber = "0123456789",
                Role = UserRole.Staff,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Email = "client@premiervenue.com",
                UserName = "client",
                PasswordHash = PasswordHasher.HashPassword("Password123!"),
                FirstName = "Client",
                LastName = "User",
                PhoneNumber = "0123456789",
                Role = UserRole.Client,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Users.AddRange(users);
        context.SaveChanges();
    }
}