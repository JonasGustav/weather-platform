using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeatherPlatform.Api.Models;
using WeatherPlatform.Api.Services;

namespace WeatherPlatform.Api.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class WeatherController(IWeatherService weatherService) : ControllerBase
{
    [HttpGet("current")]
    public async Task<ActionResult<List<WeatherWithLocationDto>>> Current([FromQuery] string city)
    {
        var result = await weatherService.GetCurrentAsync(city);
        return result is null ? NotFound($"No locations found for '{city}'.") : Ok(result);
    }

    [HttpGet("history")]
    public async Task<ActionResult<WeatherHistoryResponse>> History(
        [FromQuery] string city,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var result = await weatherService.GetHistoryAsync(city, fromDate, toDate, page, pageSize);
        return result is null ? NotFound($"No data found for '{city}'.") : Ok(result);
    }

    [HttpGet("warmest")]
    public async Task<ActionResult<WeatherWithLocationDto>> Warmest([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate) =>
        await GetExtreme(() => weatherService.GetWarmestAsync(fromDate, toDate));

    [HttpGet("coldest")]
    public async Task<ActionResult<WeatherWithLocationDto>> Coldest([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate) =>
        await GetExtreme(() => weatherService.GetColdestAsync(fromDate, toDate));

    [HttpGet("cloudiest")]
    public async Task<ActionResult<WeatherWithLocationDto>> Cloudiest([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate) =>
        await GetExtreme(() => weatherService.GetCloudiestAsync(fromDate, toDate));

    [HttpGet("highestuvi")]
    public async Task<ActionResult<WeatherWithLocationDto>> HighestUvi([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate) =>
        await GetExtreme(() => weatherService.GetHighestUviAsync(fromDate, toDate));

    [HttpGet("foggiest")]
    public async Task<ActionResult<WeatherWithLocationDto>> Foggiest([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate) =>
        await GetExtreme(() => weatherService.GetFoggiestAsync(fromDate, toDate));

    [HttpGet("windiest")]
    public async Task<ActionResult<WeatherWithLocationDto>> Windiest([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate) =>
        await GetExtreme(() => weatherService.GetWindiestAsync(fromDate, toDate));

    [HttpGet("mostrain")]
    public async Task<ActionResult<WeatherWithLocationDto>> MostRain([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate) =>
        await GetExtreme(() => weatherService.GetMostRainAsync(fromDate, toDate));

    [HttpGet("mostsnow")]
    public async Task<ActionResult<WeatherWithLocationDto>> MostSnow([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate) =>
        await GetExtreme(() => weatherService.GetMostSnowAsync(fromDate, toDate));

    private async Task<ActionResult<WeatherWithLocationDto>> GetExtreme(Func<Task<WeatherWithLocationDto?>> query)
    {
        var result = await query();
        return result is null ? NotFound("No data found.") : Ok(result);
    }
}
