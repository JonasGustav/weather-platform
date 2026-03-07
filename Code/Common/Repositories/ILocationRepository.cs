using WeatherPlatform.Common.Models;

namespace WeatherPlatform.Common.Repositories;

public interface ILocationRepository : IRepository<Location>
{
    Task<Location?> GetByCoordinatesAsync(decimal lat, decimal lon);
    Task<IEnumerable<Location>> GetAllWithLatestWeatherAsync();
}
