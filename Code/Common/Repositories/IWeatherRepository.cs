using WeatherPlatform.Common.Entities;

namespace WeatherPlatform.Common.Repositories;

public interface IWeatherRepository : IRepository<Weather>
{
    Task<Weather?> GetLatestByLocationAsync(int locationId);
    Task<IEnumerable<Weather>> GetByLocationAsync(int locationId);
}
