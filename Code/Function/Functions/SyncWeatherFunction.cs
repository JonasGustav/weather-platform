using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using WeatherPlatform.Common.Models;
using WeatherPlatform.Common.Repositories;
using WeatherPlatform.Function.Services;

namespace WeatherPlatform.Function.Functions;

public class SyncWeatherFunction(
    IOpenWeatherService openWeatherService,
    ILocationRepository locationRepository,
    IWeatherRepository weatherRepository,
    ILogger<SyncWeatherFunction> logger)
{
    private readonly IOpenWeatherService _openWeatherService = openWeatherService;
    private readonly ILocationRepository _locationRepository = locationRepository;
    private readonly IWeatherRepository _weatherRepository = weatherRepository;
    private readonly ILogger<SyncWeatherFunction> _logger = logger;

    [Function("SyncWeather")]
    public async Task Run([TimerTrigger("0 0 * * * *")] TimerInfo timer)
    {
        var locations = await _locationRepository.GetAllAsync();

        var synced = 0;
        var failed = 0;

        foreach (var location in locations)
        {
            try
            {
                var result = await _openWeatherService.GetWeatherAsync(location.Lat, location.Lon);
                if (result?.Current is null)
                {
                    _logger.LogWarning("No weather data returned for {City}", location.City);
                    failed++;
                    continue;
                }

                var current = result.Current;

                await _weatherRepository.AddAsync(new Weather
                {
                    LocationId = location.Id,
                    RecordedAt = DateTimeOffset.FromUnixTimeSeconds(current.Dt).UtcDateTime,
                    Sunrise = current.Sunrise.HasValue
                        ? DateTimeOffset.FromUnixTimeSeconds(current.Sunrise.Value).UtcDateTime
                        : null,
                    Sunset = current.Sunset.HasValue
                        ? DateTimeOffset.FromUnixTimeSeconds(current.Sunset.Value).UtcDateTime
                        : null,
                    Temp = current.Temp,
                    FeelsLike = current.FeelsLike,
                    Clouds = current.Clouds,
                    Uvi = current.Uvi,
                    Visibility = current.Visibility,
                    WindSpeed = current.WindSpeed,
                    Rain1h = current.Rain?.OneHour,
                    Snow1h = current.Snow?.OneHour,
                });

                _logger.LogInformation("Synced weather for {City} ({Temp}°C)", location.City, current.Temp);
                synced++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync weather for {City}", location.City);
                failed++;
            }
        }

        _logger.LogInformation("Sync complete. Synced: {Synced}, Failed: {Failed}", synced, failed);
    }
}
