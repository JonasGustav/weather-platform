using NSubstitute;
using WeatherPlatform.Api.Services;
using WeatherPlatform.Common.Models;
using WeatherPlatform.Common.Repositories;
using Xunit;

namespace WeatherPlatform.Api.Tests;

public class WeatherServiceTests
{
    private readonly ILocationRepository _locationRepo = Substitute.For<ILocationRepository>();
    private readonly IWeatherRepository _weatherRepo = Substitute.For<IWeatherRepository>();
    private readonly WeatherService _sut;

    public WeatherServiceTests()
    {
        _sut = new WeatherService(_locationRepo, _weatherRepo);
    }

    private static Location MakeLocation(int id = 1, string city = "Stockholm") => new()
    {
        Id = id,
        City = city,
        Lat = 12.34m,
        Lon = 43.21m
    };

    private static Weather MakeWeather(Location location, decimal temp = 5.0m) => new()
    {
        Id = location.Id,
        LocationId = location.Id,
        Location = location,
        RecordedAt = new DateTime(2026, 1, 1, 12, 0, 0),
        Temp = temp,
        FeelsLike = temp - 2,
        Clouds = 50,
        Uvi = 1.0m,
        Visibility = 5000,
        WindSpeed = 4.5m
    };


    [Fact]
    public async Task GetCurrentAsync_CityNotFound_ReturnsNull()
    {
        _locationRepo.FindByNameAsync("Unknown").Returns([]);

        var result = await _sut.GetCurrentAsync("Unknown");

        Assert.Null(result);
    }


    [Fact]
    public async Task GetHistoryAsync_CityNotFound_ReturnsNull()
    {
        _locationRepo.FindByNameAsync("Unknown").Returns([]);

        var result = await _sut.GetHistoryAsync("Unknown", null, null, 1, 50);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetHistoryAsync_NoRecords_ReturnsNull()
    {
        var location = MakeLocation();
        _locationRepo.FindByNameAsync("Stockholm").Returns([location]);
        _weatherRepo.GetHistoryAsync(Arg.Any<IEnumerable<int>>(), null, null, 1, 50)
            .Returns((Enumerable.Empty<Weather>(), 0));

        var result = await _sut.GetHistoryAsync("Stockholm", null, null, 1, 50);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetHistoryAsync_RecordsFound_ReturnsLocationOnceWithAllRecords()
    {
        var location = MakeLocation();
        _locationRepo.FindByNameAsync("Stockholm").Returns([location]);
        _weatherRepo.GetHistoryAsync(Arg.Any<IEnumerable<int>>(), null, null, 1, 50)
            .Returns(([MakeWeather(location, temp: 5.0m), MakeWeather(location, temp: 3.0m)], 2));

        var result = await _sut.GetHistoryAsync("Stockholm", null, null, 1, 50);

        Assert.NotNull(result);
        Assert.Equal("Stockholm", result.Location.City);
        Assert.Equal(2, result.Records.Count);
    }

    [Fact]
    public async Task GetHistoryAsync_CalculatesTotalPagesCorrectly()
    {
        var location = MakeLocation();
        _locationRepo.FindByNameAsync("Stockholm").Returns([location]);
        _weatherRepo.GetHistoryAsync(Arg.Any<IEnumerable<int>>(), null, null, 1, 10)
            .Returns(([MakeWeather(location)], 25));

        var result = await _sut.GetHistoryAsync("Stockholm", null, null, 1, 10);

        Assert.NotNull(result);
        Assert.Equal(3, result.TotalPages);
    }


    [Fact]
    public async Task GetWarmestAsync_NoData_ReturnsNull()
    {
        _weatherRepo.GetWarmestAsync(null, null).Returns((Weather?)null);

        var result = await _sut.GetWarmestAsync(null, null);

        Assert.Null(result);
    }
}
