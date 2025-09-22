using System.Text.Json;
using Microsoft.Extensions.Configuration;

public class WeatherService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public WeatherService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _apiKey = config["Weather:ApiKey"] ?? throw new Exception("Weather API key not found");
    }

    public async Task<object?> GetWeatherAsync(string city = "Lahore", string units = "metric")
    {
        var url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={_apiKey}&units={units}";
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        var root = JsonDocument.Parse(json).RootElement;

        var weather = root.GetProperty("weather")[0];
        var main = root.GetProperty("main");
        var wind = root.GetProperty("wind");
        var clouds = root.GetProperty("clouds");
        var sys = root.GetProperty("sys");

        return new
        {
            City = root.GetProperty("name").GetString(),
            Country = sys.GetProperty("country").GetString(),
            Temp = $"{main.GetProperty("temp").GetDouble():0.#}°C",
            FeelsLike = $"{main.GetProperty("feels_like").GetDouble():0.#}°C",
            TempMin = $"{main.GetProperty("temp_min").GetDouble():0.#}°C",
            TempMax = $"{main.GetProperty("temp_max").GetDouble():0.#}°C",
            Pressure = $"{main.GetProperty("pressure").GetInt32()} hPa",
            Humidity = $"{main.GetProperty("humidity").GetInt32()}%",
            Visibility = $"{root.GetProperty("visibility").GetInt32() / 1000.0:0.#} km",
            Wind = $"{wind.GetProperty("speed").GetDouble():0.#} m/s, {wind.GetProperty("deg").GetInt32()}°",
            Clouds = $"{clouds.GetProperty("all").GetInt32()}%",
            Condition = weather.GetProperty("description").GetString(),
            Icon = $"https://openweathermap.org/img/wn/{weather.GetProperty("icon").GetString()}@2x.png",
            Sunrise = DateTimeOffset.FromUnixTimeSeconds(sys.GetProperty("sunrise").GetInt64()).ToLocalTime().ToString("HH:mm"),
            Sunset = DateTimeOffset.FromUnixTimeSeconds(sys.GetProperty("sunset").GetInt64()).ToLocalTime().ToString("HH:mm")
        };
    }

}
