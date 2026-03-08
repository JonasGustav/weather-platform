namespace WeatherPlatform.Api.Models;

public class WeatherWithLocationDto
{
    public LocationDto Location { get; init; } = new();
    public WeatherDto Weather { get; init; } = new();
}
