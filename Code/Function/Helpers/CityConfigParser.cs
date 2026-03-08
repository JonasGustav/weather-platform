namespace WeatherPlatform.Function.Helpers;

public static class CityConfigParser
{
    // Format: "CityName,CountryCode|CityName,CountryCode"
    public static List<(string City, string CountryCode)> Parse(string raw)
    {
        var result = new List<(string, string)>();

        foreach (var entry in raw.Split('|', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = entry.Trim().Split(',');
            var city = parts[0].Trim();
            var countryCode = parts.Length > 1 ? parts[1].Trim() : "SE";
            result.Add((city, countryCode));
        }

        return result;
    }
}
