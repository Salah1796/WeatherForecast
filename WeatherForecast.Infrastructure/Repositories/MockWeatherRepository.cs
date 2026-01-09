#pragma warning disable CA1873 // Avoid potentially expensive logging

using Microsoft.Extensions.Logging;
using System.Text.Json;
using WeatherForecast.Domain.Repositories;
using WeatherForecast.Infrastructure.DTOs;
using WeatherForecastValueObject = WeatherForecast.Domain.ValueObjects.WeatherForecast;

namespace WeatherForecast.Infrastructure.Repositories;

/// <summary>
/// Mock implementation of the weather repository that loads data from JSON file.
/// </summary>
public class MockWeatherRepository : IWeatherRepository
{
    // Initialize dictionary with case-insensitive comparer to match constructor usage
    private readonly Dictionary<string, WeatherForecastValueObject> _weatherData = new(StringComparer.OrdinalIgnoreCase);
    private readonly ILogger<MockWeatherRepository>? _logger;


    /// <summary>
    /// Initializes a new instance of the <see cref="MockWeatherRepository"/> class.
    /// </summary>
    public MockWeatherRepository(ILogger<MockWeatherRepository> logger)
    {
        _logger = logger;

        var jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "weather-data.json");

        if (!File.Exists(jsonPath))
        {
            _logger?.LogError("Weather data file not found at path: {JsonPath}", jsonPath);
            return;
        }

        var jsonContent = File.ReadAllText(jsonPath);
        var weatherItems = JsonSerializer.Deserialize<List<WeatherForecastDto>>(jsonContent);
        if (weatherItems == null || weatherItems.Count == 0)
        {
            _logger?.LogError("No weather data found in the JSON file at path: {JsonPath}", jsonPath);
            return;
        }

        _weatherData = weatherItems.ToDictionary(
            item => item.City,
            item => new WeatherForecastValueObject(item.City, item.Temperature, item.Condition), 
            StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets a weather forecast by city.
    /// </summary>
    /// <param name="city">The city to get the weather forecast for.</param>
    /// <returns>The weather forecast for the specified city.</returns>
    public Task<WeatherForecastValueObject?> GetByCityAsync(string city)
    {
        if (city == null) return Task.FromResult<WeatherForecastValueObject?>(null);

        if (_weatherData.TryGetValue(city, out var weather))
        {
            return Task.FromResult<WeatherForecastValueObject?>(weather);
        }

        return Task.FromResult<WeatherForecastValueObject?>(null);
    }

}
