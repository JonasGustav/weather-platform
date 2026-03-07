using Microsoft.EntityFrameworkCore;
using WeatherPlatform.Common.Data;
using WeatherPlatform.Common.Models;
using WeatherPlatform.Common.Repositories;

var connectionString =
    Environment.GetEnvironmentVariable("ConnectionStrings__Default")
    ?? "Server=(localdb)\\mssqllocaldb;Database=WeatherPlatform;Trusted_Connection=True;";

var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlServer(connectionString)
    .Options;

await using var context = new AppDbContext(options);
var locationRepo = new LocationRepository(context);
var weatherRepo = new WeatherRepository(context);

// Seed locations
var locations = new List<Location>
{
    new() { City = "Stockholm",  Lat = 59.334591m, Lon = 18.063240m },
    new() { City = "Gothenburg", Lat = 57.708870m, Lon = 11.974560m },
    new() { City = "Malmö",      Lat = 55.604980m, Lon = 13.003820m },
    new() { City = "Uppsala",    Lat = 59.858560m, Lon = 17.638920m },
};

Console.WriteLine("Seeding locations...");
foreach (var location in locations)
{
    var existing = await locationRepo.GetByCoordinatesAsync(location.Lat, location.Lon);
    if (existing is null)
    {
        await locationRepo.AddAsync(location);
        Console.WriteLine($"  Added: {location.City}");
    }
    else
    {
        Console.WriteLine($"  Skipped (already exists): {location.City}");
        location.Id = existing.Id;
    }
}

// Seed weather readings
var now = DateTime.UtcNow;
var rng = new Random();

Console.WriteLine("Seeding weather readings...");
var allLocations = (await locationRepo.GetAllAsync()).ToList();
foreach (var location in allLocations)
{
    var weather = new Weather
    {
        LocationId = location.Id,
        RecordedAt = now,
        Sunrise    = now.Date.AddHours(6),
        Sunset     = now.Date.AddHours(18),
        Temp       = Math.Round((decimal)(rng.NextDouble() * 20 - 5), 2),
        FeelsLike  = Math.Round((decimal)(rng.NextDouble() * 20 - 7), 2),
        Clouds     = rng.Next(0, 100),
        Uvi        = Math.Round((decimal)(rng.NextDouble() * 5), 2),
        Visibility = rng.Next(1000, 10000),
        WindSpeed  = Math.Round((decimal)(rng.NextDouble() * 15), 2),
        Rain1h     = rng.Next(0, 3) == 0 ? Math.Round((decimal)(rng.NextDouble() * 5), 2) : null,
        Snow1h     = null
    };

    await weatherRepo.AddAsync(weather);
    Console.WriteLine($"  Added weather for: {location.City} (Temp: {weather.Temp}°C, Clouds: {weather.Clouds}%)");
}

Console.WriteLine("\nDone.");
