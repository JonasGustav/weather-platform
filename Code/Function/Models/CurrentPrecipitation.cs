using System.Text.Json.Serialization;

namespace WeatherPlatform.Function.Models;

public class CurrentPrecipitation
{
    [JsonPropertyName("1h")] public decimal? OneHour { get; set; }
}
