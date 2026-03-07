namespace WeatherPlatform.Common.Entities;

public class Weather
{
    public int Id { get; set; }
    public int LocationId { get; set; }
    public Location Location { get; set; } = null!;

    public DateTime RecordedAt { get; set; }
    public DateTime Sunrise { get; set; }
    public DateTime Sunset { get; set; }
    public decimal Temp { get; set; }
    public decimal FeelsLike { get; set; }
    public int Clouds { get; set; }
    public decimal Uvi { get; set; }
    public int Visibility { get; set; }
    public decimal WindSpeed { get; set; }
    public decimal? Rain1h { get; set; }
    public decimal? Snow1h { get; set; }
}
