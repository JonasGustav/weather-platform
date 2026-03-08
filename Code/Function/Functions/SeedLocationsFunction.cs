using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WeatherPlatform.Common.Models;
using WeatherPlatform.Common.Repositories;
using WeatherPlatform.Function.Helpers;
using WeatherPlatform.Function.Services;

namespace WeatherPlatform.Function.Functions;

public class SeedLocationsFunction(
    IOpenWeatherService openWeatherService,
    ILocationRepository locationRepository,
    IConfiguration configuration,
    ILogger<SeedLocationsFunction> logger)
{
    private readonly IOpenWeatherService _openWeatherService = openWeatherService;
    private readonly ILocationRepository _locationRepository = locationRepository;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<SeedLocationsFunction> _logger = logger;

    [Function("SeedLocations")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        var raw = _configuration["SeedCities"]
            ?? throw new InvalidOperationException("SeedCities app setting is required.");

        // Format: "CityName,CountryCode|CityName,CountryCode"
        var cities = CityConfigParser.Parse(raw);

        var seeded = 0;
        var skipped = 0;

        foreach (var (city, countryCode) in cities)
        {
            var result = await _openWeatherService.GetCoordinatesAsync(city, countryCode);
            if (result is null)
            {
                _logger.LogWarning("No geocoding result for {City}", city);
                continue;
            }

            var existing = await _locationRepository.GetByCoordinatesAsync(result.Lat, result.Lon);
            if (existing is not null)
            {
                _logger.LogInformation("Skipping {City} — already seeded", city);
                skipped++;
                continue;
            }

            await _locationRepository.AddAsync(new Location
            {
                City = result.Name,
                Lat = result.Lat,
                Lon = result.Lon,
            });

            _logger.LogInformation("Seeded {City} ({Lat}, {Lon})", result.Name, result.Lat, result.Lon);
            seeded++;
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new { seeded, skipped });
        return response;
    }
}
