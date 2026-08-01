namespace PremierVenue.Domain.Entities;

public class TaskItem
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskStatus Status { get; set; }
    public Priority Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public int? AssignedToId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    public Booking Booking { get; set; } = null!;
    public User? AssignedTo { get; set; }
}

public enum TaskStatus
{
    NotStarted = 1,
    InProgress = 2,
    Completed = 3,
    OnHold = 4,
    Cancelled = 5
}

public enum Priority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Urgent = 4
}