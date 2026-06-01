namespace Grab.Models;

public enum TripStatus
{
    Pending,
    Accepted,
    Completed,
    Cancelled
}

public class Trip
{
    public Guid Id { get; set; }

    public string PassengerId { get; set; } = string.Empty;

    public string? DriverId { get; set; }

    public TripStatus Status { get; set; }
}
