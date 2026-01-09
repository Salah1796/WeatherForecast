using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using WeatherForecast.Application.Common.Results;
using WeatherForecast.Application.DTOs;
using WeatherForecast.Application.Interfaces;

namespace WeatherForecast.Infrastructure.Services;

/// <summary>
/// Decorator for weather service that adds caching functionality.
/// </summary>
public class CachedWeatherService : IWeatherService
{
    private readonly IWeatherService _weatherService;
    private readonly IMemoryCache _cache;
    private readonly int _cacheTTLMinutes;

    /// <summary>
    /// Initializes a new instance of the <see cref="CachedWeatherService"/> class.
    /// </summary>
    /// <param name="weatherService">The weather service to wrap.</param>
    /// <param name="cache">The memory cache instance.</param>
    /// <param name="configuration">The configuration containing cache settings.</param>
    public CachedWeatherService(
        IWeatherService weatherService,
        IMemoryCache cache,
        IConfiguration configuration)
    {
        _weatherService = weatherService;
        _cache = cache;
        var cacheSettings = configuration.GetSection("CacheSettings");
        _cacheTTLMinutes = int.Parse(cacheSettings["WeatherCacheTTLMinutes"] ?? "30");
    }

    /// <summary>
    /// Retrieves weather forecast data for a specific city with caching.
    /// </summary>
    /// <param name="city">The name of the city to get weather data for.</param>
    /// <returns>A result containing the weather response with forecast data if successful, or error details if failed.</returns>
    public async Task<Result<WeatherResponse>> GetWeatherByCityAsync(string city)
    {
        var cacheKey = $"weather_{city.ToLowerInvariant()}";

        if (_cache.TryGetValue<Result<WeatherResponse>>(cacheKey, out var cachedResult))
        {
            return cachedResult!;
        }

        var result = await _weatherService.GetWeatherByCityAsync(city);

        if (result.Success && result.Data != null)
        {
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_cacheTTLMinutes)
            };

            _cache.Set(cacheKey, result, cacheOptions);
        }

        return result;
    }
}

