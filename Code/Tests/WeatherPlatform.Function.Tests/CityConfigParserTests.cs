using Xunit;
using WeatherPlatform.Function.Helpers;

namespace WeatherPlatform.Function.Tests;

public class CityConfigParserTests
{
    [Fact]
    public void Parse_ReturnsCityAndCountryCode()
    {
        var result = CityConfigParser.Parse("Stockholm,SE|Gothenburg,SE");

        Assert.Equal(2, result.Count);
        Assert.Equal(("Stockholm", "SE"), result[0]);
        Assert.Equal(("Gothenburg", "SE"), result[1]);
    }

    [Fact]
    public void Parse_DefaultsToSEWhenCountryCodeMissing()
    {
        var result = CityConfigParser.Parse("Stockholm");

        Assert.Single(result);
        Assert.Equal(("Stockholm", "SE"), result[0]);
    }

    [Fact]
    public void Parse_TrimsWhitespace()
    {
        var result = CityConfigParser.Parse(" Stockholm , SE | Gothenburg , SE ");

        Assert.Equal(("Stockholm", "SE"), result[0]);
        Assert.Equal(("Gothenburg", "SE"), result[1]);
    }

    [Fact]
    public void Parse_IgnoresEmptyEntries()
    {
        var result = CityConfigParser.Parse("Stockholm,SE||Gothenburg,SE");

        Assert.Equal(2, result.Count);
    }
}
