using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeatherPlatform.Api.Models;
using WeatherPlatform.Common.Models;
using WeatherPlatform.Common.Repositories;

namespace WeatherPlatform.Api.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class WeatherController(ILocationRepository locationRepo, IWeatherRepository weatherRepo) : ControllerBase
{
    [HttpGet("current")]
    public async Task<ActionResult<List<WeatherWithLocationDto>>> Current([FromQuery] string city)
    {
        var locations = (await locationRepo.FindByNameAsync(city)).ToList();
        if (locations.Count == 0) return NotFound($"No locations found for '{city}'.");

        var weather = await weatherRepo.GetLatestForLocationsAsync(locations.Select(l => l.Id));
        return Ok(weather.Select(Map).ToList());
    }

    [HttpGet("history")]
    public async Task<ActionResult<WeatherHistoryResponse>> History(
        [FromQuery] string city,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var locations = (await locationRepo.FindByNameAsync(city)).ToList();
        if (locations.Count == 0) return NotFound($"No locations found for '{city}'.");

        var (items, totalCount) = await weatherRepo.GetHistoryAsync(
            locations.Select(l => l.Id), fromDate, toDate, page, pageSize);

        var first = items.FirstOrDefault();
        if (first is null) return NotFound($"No history found for '{city}'.");

        return Ok(new WeatherHistoryResponse
        {
            Location = new LocationDto
            {
                City = first.Location.City,
                Lat = first.Location.Lat,
                Lon = first.Location.Lon
            },
            Records = items.Select(MapWeather).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    [HttpGet("warmest")]
    public Task<ActionResult<WeatherWithLocationDto>> Warmest([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate) =>
        GetExtreme(() => weatherRepo.GetWarmestAsync(fromDate, toDate));

    [HttpGet("coldest")]
    public Task<ActionResult<WeatherWithLocationDto>> Coldest([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate) =>
        GetExtreme(() => weatherRepo.GetColdestAsync(fromDate, toDate));

    [HttpGet("cloudiest")]
    public Task<ActionResult<WeatherWithLocationDto>> Cloudiest([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate) =>
        GetExtreme(() => weatherRepo.GetCloudiestAsync(fromDate, toDate));

    [HttpGet("highestuvi")]
    public Task<ActionResult<WeatherWithLocationDto>> HighestUvi([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate) =>
        GetExtreme(() => weatherRepo.GetHighestUviAsync(fromDate, toDate));

    [HttpGet("foggiest")]
    public Task<ActionResult<WeatherWithLocationDto>> Foggiest([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate) =>
        GetExtreme(() => weatherRepo.GetFoggiestAsync(fromDate, toDate));

    [HttpGet("windiest")]
    public Task<ActionResult<WeatherWithLocationDto>> Windiest([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate) =>
        GetExtreme(() => weatherRepo.GetWindiestAsync(fromDate, toDate));

    [HttpGet("mostrain")]
    public Task<ActionResult<WeatherWithLocationDto>> MostRain([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate) =>
        GetExtreme(() => weatherRepo.GetMostRainAsync(fromDate, toDate));

    [HttpGet("mostsnow")]
    public Task<ActionResult<WeatherWithLocationDto>> MostSnow([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate) =>
        GetExtreme(() => weatherRepo.GetMostSnowAsync(fromDate, toDate));

    private async Task<ActionResult<WeatherWithLocationDto>> GetExtreme(Func<Task<Weather?>> query)
    {
        var result = await query();
        if (result is null) return NotFound("No data found.");
        return Ok(Map(result));
    }

    private static WeatherWithLocationDto Map(Weather w) => new()
    {
        Location = new LocationDto
        {
            City = w.Location.City,
            Lat = w.Location.Lat,
            Lon = w.Location.Lon
        },
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
