using System;
using System.Collections.Generic;
using Grab.Models;

public class TripService
{
    private readonly List<Trip> _trips;

    private readonly DriverService _driverService;

    public TripService(DriverService driverService)
    {
        _trips = new();
        _driverService = driverService;
    }

    public void AcceptTrip(Guid tripId, string driverId)
    {
        var driver = _driverService.GetDriver(driverId);

        if (!driver.IsOnline)
        {
            Console.WriteLine("Driver offline");

            return;
        }

        if (driver.IsBusy)
        {
            Console.WriteLine("Driver busy");

            return;
        }

        var trip = _trips.First(x => x.Id == tripId);

        trip.DriverId = driverId;
        trip.Status = TripStatus.Accepted;

        _driverService.MarkBusy(driverId);

        Console.WriteLine($"{driverId} accepted");
    }

    public Trip CreateTrip(string passenger)
    {
        var trip = new Trip
        {
            Id = Guid.NewGuid(),
            PassengerId = passenger,
            Status = TripStatus.Pending
        };

        _trips.Add(trip);

        return trip;
    }
}
