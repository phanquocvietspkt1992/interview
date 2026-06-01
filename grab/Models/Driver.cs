
namespace Models;

public class Driver
{
    public string Id { get; set; } = string.Empty;

    public bool IsOnline { get; set; }

    public bool IsBusy { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }
}
