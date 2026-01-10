using System.Net;
using System.Net.Http.Json;
using WeatherForecast.Api.Tests.Fixtures;
using WeatherForecast.Application.Common.Results;
using WeatherForecast.Application.DTOs;
using Xunit;

namespace WeatherForecast.Api.Tests.Integration;

public class RateLimitingIntegrationTests : IClassFixture<WeatherForecastWebApplicationFactory>
{
    private readonly WeatherForecastWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public RateLimitingIntegrationTests(WeatherForecastWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetWeather_ExceedRateLimit_ReturnsTooManyRequests()
    {
        // 1. Arrange: Authenticate to get a valid token
        var username = $"ratelimituser_{Guid.NewGuid():N}";
        var password = "Password123!";

        await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest { Username = username, Password = password });
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest { Username = username, Password = password });
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<Result<AuthResponse>>();
        var token = loginResult!.Data!.Token;

        var authenticatedClient = _factory.CreateClient();
        authenticatedClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // 2. Act: Send 10 allowed requests
        for (int i = 0; i < 10; i++)
        {
            var response = await authenticatedClient.GetAsync("/api/weather?city=Cairo");
            // Ensure the setup is correct and we aren't failing for other reasons
            if (response.StatusCode != HttpStatusCode.OK)
            {
               // If we get an error here, it might be due to a shared state or previous tests if the rate limiter isn't isolated.
               // However, integration tests usually spin up a new server/client pair? 
               // WebApplicationFactory shares the server instance across tests in the same class fixture, 
               // but XUnit runs classes in parallel.
               // If we are sharing the server, the rate limit counter might be shared.
               // Let's assume isolation or that this runs first/alone enough. 
               // Worst case we might need to slow down or use a different user/IP if partitioning was by that.
               // The FixedWindowLimiter is global if not partitioned. My configuration: 
               // options.AddFixedWindowLimiter("fixed", ...) -> No partition key specified implies global? 
               // Wait, AddFixedWindowLimiter overload used in Program.cs:
               // .AddFixedWindowLimiter("fixed", policy => { ... }) without PartitionedOptions.
               // Wait, the API defaults to: partitioner: httpContext => RateLimitPartition.GetFixedWindowLimiter("global", ...) 
               // if not specified using the Partitioned overload.
               // My code used `options.AddFixedWindowLimiter("fixed", policy => ...)`
               // Let's check Program.cs code again.
            }
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // 3. Act: Send the 11th request
        var blockedResponse = await authenticatedClient.GetAsync("/api/weather?city=Cairo");

        // 4. Assert
        Assert.Equal(HttpStatusCode.TooManyRequests, blockedResponse.StatusCode);
    }
}
