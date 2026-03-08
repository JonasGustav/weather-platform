namespace WeatherPlatform.Api.Models;

public class WeatherHistoryResponse
{
    public LocationDto Location { get; init; } = new();
    public List<WeatherDto> Records { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
}
