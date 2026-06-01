

using Models;

public class LocationService
{

    public double Distance(double lat1, double lon1, double lat2, double lon2)
    {
        var dx = lat1 - lat2;

        var dy = lon1 - lon2;

        return Math.Sqrt(
            dx * dx +
            dy * dy);
    }
    public Driver FindNearestDriver(
               List<Driver> drivers,
               double userLat,
               double userLon)
    {
        var nearestDriver = drivers.First();
        var nearestDistance = Distance(
            userLat,
            userLon,
            nearestDriver.Latitude,
            nearestDriver.Longitude);

        foreach (var driver in drivers)
        {
            var distance = Distance(
                userLat,
                userLon,
                driver.Latitude,
                driver.Longitude);

            Console.WriteLine($"{driver.Id} = {distance}");
            if (distance < nearestDistance)
            {
                nearestDriver = driver;
                nearestDistance = distance;
            }
        }

        return nearestDriver;
    }
}
