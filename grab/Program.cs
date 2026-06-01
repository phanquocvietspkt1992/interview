var locationService = new RedisLocationService();

await locationService.ClearIndex();
await locationService.UpdateLocation("DRIVER_A", 10.7800, 106.7000);
await locationService.UpdateLocation("DRIVER_B", 10.7750, 106.6900);
await locationService.UpdateLocation("DRIVER_C", 10.7600, 106.6800);

var passengerLat = 10.7760;
var passengerLon = 106.6950;

var req1 = locationService.ReserveNearestDriver(passengerLat, passengerLon, 3, 5, 20);
var req2 = locationService.ReserveNearestDriver(passengerLat, passengerLon, 3, 5, 20);

await Task.WhenAll(req1, req2);

Console.WriteLine($"Request 1 reserved: {req1.Result ?? "NONE"}");
Console.WriteLine($"Request 2 reserved: {req2.Result ?? "NONE"}");

if (!string.IsNullOrWhiteSpace(req1.Result))
{
    await locationService.ReleaseReservation(req1.Result);
}

if (!string.IsNullOrWhiteSpace(req2.Result))
{
    await locationService.ReleaseReservation(req2.Result);
}