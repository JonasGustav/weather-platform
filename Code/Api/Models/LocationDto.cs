namespace WeatherPlatform.Api.Models;

public class LocationDto
{
    public string City { get; init; } = string.Empty;
    public decimal Lat { get; init; }
    public decimal Lon { get; init; }
}
