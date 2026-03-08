using System.Text.Json.Serialization;

namespace WeatherPlatform.Function.Models;

public class CurrentWeather
{
    [JsonPropertyName("dt")] public long Dt { get; set; }
    [JsonPropertyName("sunrise")] public long? Sunrise { get; set; }
    [JsonPropertyName("sunset")] public long? Sunset { get; set; }
    [JsonPropertyName("temp")] public decimal Temp { get; set; }
    [JsonPropertyName("feels_like")] public decimal FeelsLike { get; set; }
    [JsonPropertyName("pressure")] public int Pressure { get; set; }
    [JsonPropertyName("humidity")] public int Humidity { get; set; }
    [JsonPropertyName("uvi")] public decimal Uvi { get; set; }
    [JsonPropertyName("clouds")] public int Clouds { get; set; }
    [JsonPropertyName("visibility")] public int Visibility { get; set; }
    [JsonPropertyName("wind_speed")] public decimal WindSpeed { get; set; }
    [JsonPropertyName("wind_deg")] public int WindDeg { get; set; }
    [JsonPropertyName("wind_gust")] public decimal? WindGust { get; set; }
    [JsonPropertyName("weather")] public WeatherCondition[]? Weather { get; set; }
    [JsonPropertyName("rain")] public CurrentPrecipitation? Rain { get; set; }
    [JsonPropertyName("snow")] public CurrentPrecipitation? Snow { get; set; }
}
