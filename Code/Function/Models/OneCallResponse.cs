using System.Text.Json.Serialization;

namespace WeatherPlatform.Function.Models;

public class OneCallResponse
{
    [JsonPropertyName("lat")] public decimal Lat { get; set; }
    [JsonPropertyName("lon")] public decimal Lon { get; set; }
    [JsonPropertyName("timezone")] public string Timezone { get; set; } = string.Empty;
    [JsonPropertyName("timezone_offset")] public int TimezoneOffset { get; set; }
    [JsonPropertyName("current")] public CurrentWeather? Current { get; set; }
}
