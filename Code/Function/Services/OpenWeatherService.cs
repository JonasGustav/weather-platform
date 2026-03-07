using System.Text.Json;
using Azure.Security.KeyVault.Secrets;
using WeatherPlatform.Function.Models;

namespace WeatherPlatform.Function.Services;

public class OpenWeatherService : IOpenWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly SecretClient _secretClient;
    private string? _apiKey;

    public OpenWeatherService(HttpClient httpClient, SecretClient secretClient)
    {
        _httpClient = httpClient;
        _secretClient = secretClient;
    }

    public async Task<GeocodingResult?> GetCoordinatesAsync(string city, string countryCode)
    {
        await EnsureApiKeyAsync();

        var url = $"http://api.openweathermap.org/geo/1.0/direct?q={Uri.EscapeDataString(city)},{countryCode}&limit=1&appid={_apiKey}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var results = await JsonSerializer.DeserializeAsync<GeocodingResult[]>(
            await response.Content.ReadAsStreamAsync());

        return results?.Length > 0 ? results[0] : null;
    }

    public async Task<OneCallResponse?> GetWeatherAsync(decimal lat, decimal lon)
    {
        await EnsureApiKeyAsync();

        var url = $"https://api.openweathermap.org/data/3.0/onecall?lat={lat}&lon={lon}&exclude=minutely,hourly,daily,alerts&units=metric&appid={_apiKey}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        return await JsonSerializer.DeserializeAsync<OneCallResponse>(
            await response.Content.ReadAsStreamAsync());
    }

    private async Task EnsureApiKeyAsync()
    {
        _apiKey ??= (await _secretClient.GetSecretAsync("OpenWeatherApiKey")).Value.Value;
    }
}
