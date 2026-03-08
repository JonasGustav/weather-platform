using WeatherPlatform.Common.Models;

namespace WeatherPlatform.Common.Repositories;

public interface IWeatherRepository : IRepository<Weather>
{
    Task<Weather?> GetLatestByLocationAsync(int locationId);
    Task<IEnumerable<Weather>> GetByLocationAsync(int locationId);
    Task<IEnumerable<Weather>> GetLatestForLocationsAsync(IEnumerable<int> locationIds);
    Task<(IEnumerable<Weather> Items, int TotalCount)> GetHistoryAsync(IEnumerable<int> locationIds, DateTime? from, DateTime? to, int page, int pageSize);
    Task<Weather?> GetWarmestAsync(DateTime? from, DateTime? to);
    Task<Weather?> GetCloudiestAsync(DateTime? from, DateTime? to);
    Task<Weather?> GetHighestUviAsync(DateTime? from, DateTime? to);
    Task<Weather?> GetFoggiestAsync(DateTime? from, DateTime? to);
    Task<Weather?> GetWindiestAsync(DateTime? from, DateTime? to);
    Task<Weather?> GetMostRainAsync(DateTime? from, DateTime? to);
    Task<Weather?> GetMostSnowAsync(DateTime? from, DateTime? to);
}
