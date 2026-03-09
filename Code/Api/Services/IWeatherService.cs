using WeatherPlatform.Api.Models;

namespace WeatherPlatform.Api.Services;

public interface IWeatherService
{
    Task<List<WeatherWithLocationDto>?> GetCurrentAsync(string city);
    Task<WeatherHistoryResponse?> GetHistoryAsync(string city, DateTime? from, DateTime? to, int page, int pageSize);
    Task<WeatherWithLocationDto?> GetWarmestAsync(DateTime? from, DateTime? to);
    Task<WeatherWithLocationDto?> GetColdestAsync(DateTime? from, DateTime? to);
    Task<WeatherWithLocationDto?> GetCloudiestAsync(DateTime? from, DateTime? to);
    Task<WeatherWithLocationDto?> GetHighestUviAsync(DateTime? from, DateTime? to);
    Task<WeatherWithLocationDto?> GetFoggiestAsync(DateTime? from, DateTime? to);
    Task<WeatherWithLocationDto?> GetWindiestAsync(DateTime? from, DateTime? to);
    Task<WeatherWithLocationDto?> GetMostRainAsync(DateTime? from, DateTime? to);
    Task<WeatherWithLocationDto?> GetMostSnowAsync(DateTime? from, DateTime? to);
}
