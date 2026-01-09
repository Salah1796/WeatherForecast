using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeatherForecast.Application.DTOs;
using WeatherForecast.Application.Interfaces;

namespace WeatherForecast.Api.Controllers;

/// <summary>
/// Weather controller for retrieving weather forecast data.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WeatherController : ControllerBase
{
    private readonly IWeatherService _weatherService;

    /// <summary>
    /// Initializes a new instance of the <see cref="WeatherController"/> class.
    /// </summary>
    /// <param name="weatherService">The weather service.</param>
    public WeatherController(IWeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    /// <summary>
    /// Gets the weather forecast for a specific city.
    /// </summary>
    /// <param name="city">The name of the city to get weather data for.</param>
    /// <returns>The weather forecast data for the specified city.</returns>
    /// <response code="200">Weather data retrieved successfully.</response>
    /// <response code="400">City name is required or invalid.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="404">Weather data not found for the specified city.</response>
    [HttpGet]
    [ProducesResponseType(typeof(WeatherResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWeatherByCity([FromQuery] string city)
    {
        var result = await _weatherService.GetWeatherByCityAsync(city);

        return StatusCode((int)result.StatusCode, result);
    }
}
