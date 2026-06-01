

using StackExchange.Redis;

public class RedisLocationService
{
    private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    private const string DriverGeoKey = "drivers:geo";
    private const string DriverReservationPrefix = "drivers:reservation:";

    public RedisLocationService()
    {
        var options = ConfigurationOptions.Parse("localhost:6379");
        options.AbortOnConnectFail = false;
        options.ConnectRetry = 3;
        options.ConnectTimeout = 3000;
        options.AsyncTimeout = 1000;
        options.SyncTimeout = 1000;

        _redis = ConnectionMultiplexer.Connect(options);
        _db = _redis.GetDatabase();
    }

    private bool IsRedisReady(string operation)
    {
        if (_redis.IsConnected)
        {
            return true;
        }

        Console.WriteLine($"Redis unavailable: skip {operation}");
        return false;
    }

    public async Task UpdateLocation(string driverId, double lat, double lon)
    {
        if (!IsRedisReady("UpdateLocation"))
        {
            return;
        }

        try
        {
            await _db.GeoAddAsync(DriverGeoKey, lon, lat, driverId);
            Console.WriteLine($"Location updated: {driverId} -> ({lat}, {lon})");
        }
        catch (RedisConnectionException ex)
        {
            Console.WriteLine($"Redis unavailable while updating {driverId}: {ex.FailureType}");
        }
    }

    public async Task<string?> FindNearestDriver(double userLatitude, double userLongitude, double radiusKm = 5)
    {
        if (!IsRedisReady("FindNearestDriver"))
        {
            return null;
        }

        try
        {
            var results = await _db.GeoRadiusAsync(
                DriverGeoKey,
                userLongitude,
                userLatitude,
                radiusKm,
                unit: GeoUnit.Kilometers,
                count: 1,
                order: Order.Ascending);

            var nearest = results.FirstOrDefault();
            if (nearest.Member.IsNullOrEmpty) return null;

            return nearest.Member.ToString();
        }
        catch (RedisConnectionException ex)
        {
            Console.WriteLine($"Redis unavailable while querying nearest driver: {ex.FailureType}");
            return null;
        }
    }

    public async Task<List<string>> FindNearbyDrivers(
        double userLatitude,
        double userLongitude,
        double radiusKm = 5,
        int count = 10)
    {
        if (!IsRedisReady("FindNearbyDrivers"))
        {
            return new List<string>();
        }

        try
        {
            var results = await _db.GeoRadiusAsync(
                DriverGeoKey,
                userLongitude,
                userLatitude,
                radiusKm,
                unit: GeoUnit.Kilometers,
                count: count,
                order: Order.Ascending);

            return results
                .Where(x => !x.Member.IsNullOrEmpty)
                .Select(x => x.Member.ToString())
                .ToList();
        }
        catch (RedisConnectionException ex)
        {
            Console.WriteLine($"Redis unavailable while querying nearby drivers: {ex.FailureType}");
            return new List<string>();
        }
    }

    public async Task ClearIndex()
    {
        if (!IsRedisReady("ClearIndex"))
        {
            return;
        }

        try
        {
            await _db.KeyDeleteAsync(DriverGeoKey);
        }
        catch (RedisConnectionException ex)
        {
            Console.WriteLine($"Redis unavailable while clearing index: {ex.FailureType}");
        }
    }

    public async Task<string?> ReserveNearestDriver(
double userLatitude, double userLongitude, double radiusKm = 5, int searchCount = 10, int reservationSeconds = 30
    )
    {
        if (!IsRedisReady("ReserverNearestDriver"))
            return null;
        var nearbyDrivers = await FindNearbyDrivers(userLatitude, userLongitude, radiusKm, searchCount);
        foreach (var driverId in nearbyDrivers)
        {
            var lockKey = $"{DriverReservationPrefix}{driverId}";
            var reserved = await _db.StringSetAsync(
                lockKey,
                "reserved",
                expiry: TimeSpan.FromSeconds(reservationSeconds),
                when: When.NotExists);

            if (reserved)
            {
                return driverId;
            }
        }
        return null;

    }
    public async Task ReleaseReservation(string driverId)
    {
        if (!IsRedisReady("ReleaseReservation"))
        {
            return;
        }

        var lockKey = $"{DriverReservationPrefix}{driverId}";
        await _db.KeyDeleteAsync(lockKey);
    }

}