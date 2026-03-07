namespace WeatherPlatform.Common.Models;

public class Location
{
    public int Id { get; set; }
    public string City { get; set; } = string.Empty;
    public decimal Lat { get; set; }
    public decimal Lon { get; set; }

    public ICollection<Weather> WeatherReadings { get; set; } = [];
}
