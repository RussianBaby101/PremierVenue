using Microsoft.EntityFrameworkCore;
using PremierVenue.Domain.Entities;

namespace PremierVenue.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Venue> Venues { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Amenity> Amenities { get; set; }
    public DbSet<VenueAmenity> VenueAmenities { get; set; }
    public DbSet<VenuePhoto> VenuePhotos { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<BookingDocument> BookingDocuments { get; set; }
    public DbSet<TaskItem> Tasks { get; set; }
    public DbSet<Availability> Availabilities { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<SavedVenue> SavedVenues { get; set; }
    public DbSet<VenueEventType> VenueEventTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
        });

        // Venue configuration
        modelBuilder.Entity<Venue>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Province).HasMaxLength(100);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.Latitude).HasPrecision(18, 2);
            entity.Property(e => e.Longitude).HasPrecision(18, 2);
            entity.Property(e => e.BasePricePerDay).HasPrecision(18, 2);
            var customAmenities = entity.Property(e => e.CustomAmenities)
                .HasConversion(
                    amenities => System.Text.Json.JsonSerializer.Serialize(amenities, (System.Text.Json.JsonSerializerOptions?)null),
                    value => System.Text.Json.JsonSerializer.Deserialize<List<string>>(value, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>());
            customAmenities.Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<string>>(
                (left, right) => left != null && right != null && left.SequenceEqual(right),
                value => value.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())),
                value => value.ToList()));

            var supportedServices = entity.Property(e => e.SupportedServices)
                .HasConversion(
                    services => System.Text.Json.JsonSerializer.Serialize(services, (System.Text.Json.JsonSerializerOptions?)null),
                    value => System.Text.Json.JsonSerializer.Deserialize<List<string>>(value, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>());
            supportedServices.Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<string>>(
                (left, right) => left != null && right != null && left.SequenceEqual(right),
                value => value.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())),
                value => value.ToList()));
        });

        // Booking configuration
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasIndex(e => e.ReferenceNumber).IsUnique();
            entity.Property(e => e.ReferenceNumber).HasMaxLength(50);
            entity.Property(e => e.SpecialRequirements).HasMaxLength(2000);
            entity.Property(e => e.EstimatedBudget).HasPrecision(18, 2);
            entity.Property(e => e.FinalQuote).HasPrecision(18, 2);
            entity.Property(e => e.DepositAmount).HasPrecision(18, 2);
            entity.Property(e => e.BalanceAmount).HasPrecision(18, 2);
            entity.Property(e => e.RefundAmount).HasPrecision(18, 2);
            entity.Property(e => e.CancellationFeeAmount).HasPrecision(18, 2);
        });

        // Amenity configuration
        modelBuilder.Entity<Amenity>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Icon).HasMaxLength(50);
        });

        // VenueAmenity configuration (many-to-many)
        modelBuilder.Entity<VenueAmenity>(entity =>
        {
            entity.HasOne(va => va.Venue)
                .WithMany(v => v.Amenities)
                .HasForeignKey(va => va.VenueId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(va => va.Amenity)
                .WithMany(a => a.VenueAmenities)
                .HasForeignKey(va => va.AmenityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.AdditionalCost).HasPrecision(18, 2);
        });

        modelBuilder.Entity<VenueEventType>(entity =>
        {
            entity.HasKey(vet => new { vet.VenueId, vet.EventType });

            entity.HasOne(vet => vet.Venue)
                .WithMany(v => v.EventTypes)
                .HasForeignKey(vet => vet.VenueId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SavedVenue>(entity =>
        {
            entity.HasIndex(sv => new { sv.UserId, sv.VenueId }).IsUnique();

            entity.HasOne(sv => sv.User)
                .WithMany(u => u.SavedVenues)
                .HasForeignKey(sv => sv.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(sv => sv.Venue)
                .WithMany(v => v.SavedByUsers)
                .HasForeignKey(sv => sv.VenueId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // VenuePhoto configuration
        modelBuilder.Entity<VenuePhoto>(entity =>
        {
            entity.HasOne(vp => vp.Venue)
                .WithMany(v => v.Photos)
                .HasForeignKey(vp => vp.VenueId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.Url).HasMaxLength(1000);
            entity.Property(e => e.Caption).HasMaxLength(500);
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.ContentType).HasMaxLength(100);
        });

        // Payment configuration
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasOne(p => p.Booking)
                .WithMany(b => b.Payments)
                .HasForeignKey(p => p.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.TransactionReference).HasMaxLength(200);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
        });

        // Message configuration
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasOne(m => m.Booking)
                .WithMany(b => b.Messages)
                .HasForeignKey(m => m.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(m => m.Receiver)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.Content).HasMaxLength(2000);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.Property(e => e.AdditionalServices).HasMaxLength(2000);
            entity.Property(e => e.CancellationPolicy).HasMaxLength(4000);
            entity.Property(e => e.CancellationPolicyCode).HasMaxLength(50);
            entity.Property(e => e.RefundStatus).HasMaxLength(50);
            entity.Property(e => e.CancellationFeeStatus).HasMaxLength(50);
        });

        // BookingDocument configuration
        modelBuilder.Entity<BookingDocument>(entity =>
        {
            entity.HasOne(bd => bd.Booking)
                .WithMany(b => b.Documents)
                .HasForeignKey(bd => bd.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.Url).HasMaxLength(1000);
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        // TaskItem configuration
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasOne(t => t.Booking)
                .WithMany(b => b.Tasks)
                .HasForeignKey(t => t.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(t => t.AssignedTo)
                .WithMany()
                .HasForeignKey(t => t.AssignedToId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
        });

        // Availability configuration
        modelBuilder.Entity<Availability>(entity =>
        {
            entity.HasOne(a => a.Venue)
                .WithMany(v => v.Availabilities)
                .HasForeignKey(a => a.VenueId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(a => new { a.VenueId, a.Date }).IsUnique();
        });

        // AuditLog configuration
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasOne(al => al.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(al => al.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.Action).HasMaxLength(100);
            entity.Property(e => e.EntityName).HasMaxLength(100);
            entity.Property(e => e.IpAddress).HasMaxLength(50);
        });

        // Notification configuration
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Message).HasMaxLength(1000);
            entity.Property(e => e.ActionUrl).HasMaxLength(500);
        });
    }
}