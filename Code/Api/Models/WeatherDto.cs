namespace WeatherPlatform.Api.Models;

public class WeatherDto
{
    public DateTime RecordedAt { get; init; }
    public DateTime? Sunrise { get; init; }
    public DateTime? Sunset { get; init; }
    public decimal Temp { get; init; }
    public decimal FeelsLike { get; init; }
    public int Clouds { get; init; }
    public decimal Uvi { get; init; }
    public int Visibility { get; init; }
    public decimal WindSpeed { get; init; }
    public decimal? Rain1h { get; init; }
    public decimal? Snow1h { get; init; }
}
