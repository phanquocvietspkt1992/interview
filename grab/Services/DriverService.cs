using Models;
public class DriverService
{
    private readonly List<Driver> _drivers = new();

    public void RegisterDriver(
     string id,
     bool online,
     double lat,
     double lon)
    {
        _drivers.Add(
            new Driver
            {
                Id = id,
                IsOnline = online,
                Latitude = lat,
                Longitude = lon
            });
    }

    public List<Driver>
        GetAvailableDrivers()
    {
        return _drivers
            .Where(x =>
                x.IsOnline &&
                !x.IsBusy)
            .ToList();
    }

    public Driver GetDriver(
        string id)
    {
        return _drivers
            .First(x => x.Id == id);
    }

    public void MarkBusy(
        string id)
    {
        GetDriver(id)
            .IsBusy = true;
    }

    public void UpdateLocation(
        string id,
        double lat,
        double lon)
    {
        var driver = GetDriver(id);

        driver.Latitude = lat;
        driver.Longitude = lon;
        Console.WriteLine($"{id}: ({lat} {lon}) ");
    }
}
