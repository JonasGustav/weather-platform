using WeatherPlatform.Api.Models;
using WeatherPlatform.Common.Models;
using WeatherPlatform.Common.Repositories;

namespace WeatherPlatform.Api.Services;

public class WeatherService(ILocationRepository locationRepo, IWeatherRepository weatherRepo) : IWeatherService
{
    public async Task<List<WeatherWithLocationDto>?> GetCurrentAsync(string city)
    {
        var locations = (await locationRepo.FindByNameAsync(city)).ToList();
        if (locations.Count == 0) return null;

        var weather = await weatherRepo.GetLatestForLocationsAsync(locations.Select(l => l.Id));
        return weather.Select(Map).ToList();
    }

    public async Task<WeatherHistoryResponse?> GetHistoryAsync(string city, DateTime? from, DateTime? to, int page, int pageSize)
    {
        var locations = (await locationRepo.FindByNameAsync(city)).ToList();
        if (locations.Count == 0) return null;

        var (items, totalCount) = await weatherRepo.GetHistoryAsync(locations.Select(l => l.Id), from, to, page, pageSize);

        var first = items.FirstOrDefault();
        if (first is null) return null;

        return new WeatherHistoryResponse
        {
            Location = new LocationDto { City = first.Location.City, Lat = first.Location.Lat, Lon = first.Location.Lon },
            Records = items.Select(MapWeather).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<WeatherWithLocationDto?> GetWarmestAsync(DateTime? from, DateTime? to) =>
        Map(await weatherRepo.GetWarmestAsync(from, to));

    public async Task<WeatherWithLocationDto?> GetColdestAsync(DateTime? from, DateTime? to) =>
        Map(await weatherRepo.GetColdestAsync(from, to));

    public async Task<WeatherWithLocationDto?> GetCloudiestAsync(DateTime? from, DateTime? to) =>
        Map(await weatherRepo.GetCloudiestAsync(from, to));

    public async Task<WeatherWithLocationDto?> GetHighestUviAsync(DateTime? from, DateTime? to) =>
        Map(await weatherRepo.GetHighestUviAsync(from, to));

    public async Task<WeatherWithLocationDto?> GetFoggiestAsync(DateTime? from, DateTime? to) =>
        Map(await weatherRepo.GetFoggiestAsync(from, to));

    public async Task<WeatherWithLocationDto?> GetWindiestAsync(DateTime? from, DateTime? to) =>
        Map(await weatherRepo.GetWindiestAsync(from, to));

    public async Task<WeatherWithLocationDto?> GetMostRainAsync(DateTime? from, DateTime? to) =>
        Map(await weatherRepo.GetMostRainAsync(from, to));

    public async Task<WeatherWithLocationDto?> GetMostSnowAsync(DateTime? from, DateTime? to) =>
        Map(await weatherRepo.GetMostSnowAsync(from, to));

    private static WeatherWithLocationDto? Map(Weather? w) =>
        w is null ? null : new WeatherWithLocationDto
        {
            Location = new LocationDto { City = w.Location.City, Lat = w.Location.Lat, Lon = w.Location.Lon },
            Weather = MapWeather(w)
        };

    private static WeatherDto MapWeather(Weather w) => new()
    {
        RecordedAt = w.RecordedAt,
        Sunrise = w.Sunrise,
        Sunset = w.Sunset,
        Temp = w.Temp,
        FeelsLike = w.FeelsLike,
        Clouds = w.Clouds,
        Uvi = w.Uvi,
        Visibility = w.Visibility,
        WindSpeed = w.WindSpeed,
        Rain1h = w.Rain1h,
        Snow1h = w.Snow1h
    };
}
