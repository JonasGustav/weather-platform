using System.Text.Json.Serialization;

namespace WeatherPlatform.Function.Models;

public class GeocodingResult
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("lat")]
    public decimal Lat { get; set; }

    [JsonPropertyName("lon")]
    public decimal Lon { get; set; }

    [JsonPropertyName("country")]
    public string Country { get; set; } = string.Empty;
}
