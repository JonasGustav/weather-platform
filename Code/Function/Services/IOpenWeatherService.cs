using WeatherPlatform.Function.Models;

namespace WeatherPlatform.Function.Services;

public interface IOpenWeatherService
{
    Task<GeocodingResult?> GetCoordinatesAsync(string city, string countryCode);
    Task<OneCallResponse?> GetWeatherAsync(decimal lat, decimal lon);
}
