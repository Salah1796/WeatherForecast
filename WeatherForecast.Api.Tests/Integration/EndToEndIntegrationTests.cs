//using System.Net;
//using System.Net.Http.Json;
//using WeatherForecast.Api.Tests.Fixtures;
//using WeatherForecast.Application.DTOs;
//using Xunit;

//namespace WeatherForecast.Api.Tests.Integration;

///// <summary>
///// End-to-end integration tests covering the complete API workflow.
///// Tests registration, login, and weather service functionality as a complete user journey.
///// </summary>
//public class EndToEndIntegrationTests : IClassFixture<WeatherForecastWebApplicationFactory>
//{
//    private readonly WeatherForecastWebApplicationFactory _factory;
//    private readonly HttpClient _client;

//    public EndToEndIntegrationTests(WeatherForecastWebApplicationFactory factory)
//    {
//        _factory = factory;
//        _client = factory.CreateClient();
//    }

//    [Fact]
//    public async Task UserJourney_CompleteFlow_RegistrationThroughWeatherAccess()
//    {
//        // This test simulates a complete user journey:
//        // 1. User registers with credentials
//        // 2. User logs in
//        // 3. User accesses protected weather endpoint
//        // 4. User attempts to access weather without authorization
//        // 5. User logs in again and accesses protected endpoint

//        // Arrange
//        var username = "journeyuser";
//        var password = "SecurePassword123!";

//        // Step 1: Register user
//        var registerRequest = new RegisterRequest
//        {
//            Username = username,
//            Password = password
//        };

//        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
//        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

//        var registerContent = await registerResponse.Content.ReadAsAsync<AuthResponse>();
//        Assert.NotNull(registerContent);
//        Assert.NotNull(registerContent.Token);
//        Assert.Equal(username, registerContent.Username);
//        var userId = registerContent.UserId;

//        // Step 2: Login user
//        var loginRequest = new LoginRequest
//        {
//            Username = username,
//            Password = password
//        };

//        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
//        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

//        var loginContent = await loginResponse.Content.ReadAsAsync<AuthResponse>();
//        Assert.NotNull(loginContent);
//        Assert.Equal(userId, loginContent.UserId);
//        Assert.Equal(username, loginContent.Username);

//        // Step 3: Access protected weather endpoint with token
//        var authenticatedClient = _factory.CreateClient();
//        authenticatedClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {loginContent.Token}");

//        var weatherResponse = await authenticatedClient.GetAsync("/api/weather?city=London");
//        Assert.True(weatherResponse.StatusCode == HttpStatusCode.OK || weatherResponse.StatusCode == HttpStatusCode.NotFound,
//            $"Expected OK or NotFound, but got {weatherResponse.StatusCode}");

//        // Step 4: Try to access protected endpoint without token
//        var unauthorizedResponse = await _client.GetAsync("/api/weather?city=London");
//        Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedResponse.StatusCode);

//        // Step 5: Login again and verify token still works
//        var secondLoginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
//        Assert.Equal(HttpStatusCode.OK, secondLoginResponse.StatusCode);

//        var secondLoginContent = await secondLoginResponse.Content.ReadAsAsync<AuthResponse>();
//        Assert.NotNull(secondLoginContent.Token);

//        var authenticatedClient2 = _factory.CreateClient();
//        authenticatedClient2.DefaultRequestHeaders.Add("Authorization", $"Bearer {secondLoginContent.Token}");

//        var secondWeatherResponse = await authenticatedClient2.GetAsync("/api/weather?city=Paris");
//        Assert.True(secondWeatherResponse.StatusCode == HttpStatusCode.OK || secondWeatherResponse.StatusCode == HttpStatusCode.NotFound,
//            $"Expected OK or NotFound, but got {secondWeatherResponse.StatusCode}");
//    }

//    [Fact]
//    public async Task ConcurrentUsers_MultipleRegistrationsAndLogins_AllSucceed()
//    {
//        // This test verifies that multiple users can register and login concurrently
//        // without interfering with each other.

//        var userTasks = new List<Task<(bool registerSuccess, bool loginSuccess)>>();

//        for (int i = 0; i < 5; i++)
//        {
//            var index = i;
//            var task = Task.Run(async () =>
//            {
//                var username = $"concurrentuser{index}";
//                var password = $"Password{index}!";

//                // Register
//                var registerRequest = new RegisterRequest { Username = username, Password = password };
//                var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
//                var registerSuccess = registerResponse.StatusCode == HttpStatusCode.OK;

//                // Login
//                var loginRequest = new LoginRequest { Username = username, Password = password };
//                var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
//                var loginSuccess = loginResponse.StatusCode == HttpStatusCode.OK;

//                return (registerSuccess, loginSuccess);
//            });

//            userTasks.Add(task);
//        }

//        var results = await Task.WhenAll(userTasks);

//        // Assert - all registrations and logins should succeed
//        foreach (var (registerSuccess, loginSuccess) in results)
//        {
//            Assert.True(registerSuccess, "At least one registration failed");
//            Assert.True(loginSuccess, "At least one login failed");
//        }
//    }

//    [Fact]
//    public async Task InvalidCredentialSequence_RegisterWithValidThenLoginWithInvalid()
//    {
//        // Arrange
//        var username = "validuser";
//        var correctPassword = "CorrectPassword123!";
//        var wrongPassword = "WrongPassword456!";

//        // Register with valid credentials
//        var registerRequest = new RegisterRequest { Username = username, Password = correctPassword };
//        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
//        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

//        // Act - Try to login with wrong password
//        var wrongLoginRequest = new LoginRequest { Username = username, Password = wrongPassword };
//        var wrongLoginResponse = await _client.PostAsJsonAsync("/api/auth/login", wrongLoginRequest);

//        // Assert
//        Assert.Equal(HttpStatusCode.Unauthorized, wrongLoginResponse.StatusCode);

//        // Act - Login with correct password should still work
//        var correctLoginRequest = new LoginRequest { Username = username, Password = correctPassword };
//        var correctLoginResponse = await _client.PostAsJsonAsync("/api/auth/login", correctLoginRequest);

//        // Assert
//        Assert.Equal(HttpStatusCode.OK, correctLoginResponse.StatusCode);
//    }

//    [Fact]
//    public async Task WeatherDataAccess_MultipleRequestsForDifferentCities()
//    {
//        // Arrange
//        var username = "citiesuser";
//        var password = "Password123!";

//        // Register and login
//        var registerRequest = new RegisterRequest { Username = username, Password = password };
//        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

//        var loginRequest = new LoginRequest { Username = username, Password = password };
//        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
//        var loginContent = await loginResponse.Content.ReadAsAsync<AuthResponse>();

//        var authenticatedClient = _factory.CreateClient();
//        authenticatedClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {loginContent.Token}");

//        // Act - Request weather for multiple cities
//        var cities = new[] { "London", "NewYork", "Tokyo", "Sydney", "Dubai" };
//        var responses = new Dictionary<string, HttpStatusCode>();

//        foreach (var city in cities)
//        {
//            var response = await authenticatedClient.GetAsync($"/api/weather?city={city}");
//            responses[city] = response.StatusCode;
//        }

//        // Assert - All requests should either succeed or return NotFound
//        foreach (var (city, statusCode) in responses)
//        {
//            Assert.True(
//                statusCode == HttpStatusCode.OK || statusCode == HttpStatusCode.NotFound,
//                $"City {city} returned unexpected status code {statusCode}");
//        }
//    }

//    [Fact]
//    public async Task SecurityBoundary_ExpiredTokenBehavior()
//    {
//        // Arrange
//        var username = "securityuser";
//        var password = "Password123!";

//        // Register and login
//        var registerRequest = new RegisterRequest { Username = username, Password = password };
//        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

//        var loginRequest = new LoginRequest { Username = username, Password = password };
//        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
//        var loginContent = await loginResponse.Content.ReadAsAsync<AuthResponse>();

//        // Act - Use an obviously invalid/expired token format
//        var invalidClient = _factory.CreateClient();
//        invalidClient.DefaultRequestHeaders.Add("Authorization", "Bearer invalid.token.format");

//        var weatherResponse = await invalidClient.GetAsync("/api/weather?city=London");

//        // Assert
//        Assert.Equal(HttpStatusCode.Unauthorized, weatherResponse.StatusCode);
//    }

//    [Fact]
//    public async Task RegistrationValidation_VariousInvalidInputs()
//    {
//        // Test various invalid registration scenarios

//        var invalidRequests = new[]
//        {
//            new RegisterRequest { Username = "", Password = "Password123!" },
//            new RegisterRequest { Username = "validuser", Password = "" },
//            new RegisterRequest { Username = "   ", Password = "Password123!" },
//            new RegisterRequest { Username = "user", Password = "short" },
//        };

//        foreach (var request in invalidRequests)
//        {
//            var response = await _client.PostAsJsonAsync("/api/auth/register", request);
//            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode,
//                $"Expected BadRequest for request: Username='{request.Username}', Password='{request.Password}'");
//        }
//    }

//    [Fact]
//    public async Task AuthenticationFlow_TokenUsableOnlyOnceForMultipleRequests()
//    {
//        // Arrange
//        var username = "multiuseuser";
//        var password = "Password123!";

//        // Register and login
//        await _client.PostAsJsonAsync("/api/auth/register", 
//            new RegisterRequest { Username = username, Password = password });

//        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login",
//            new LoginRequest { Username = username, Password = password });
//        var loginContent = await loginResponse.Content.ReadAsAsync<AuthResponse>();

//        // Act - Use the same token for multiple requests
//        var authenticatedClient = _factory.CreateClient();
//        authenticatedClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {loginContent.Token}");

//        var response1 = await authenticatedClient.GetAsync("/api/weather?city=London");
//        var response2 = await authenticatedClient.GetAsync("/api/weather?city=Paris");
//        var response3 = await authenticatedClient.GetAsync("/api/weather?city=Tokyo");

//        // Assert - Same token should work for all requests
//        Assert.True(response1.StatusCode == HttpStatusCode.OK || response1.StatusCode == HttpStatusCode.NotFound);
//        Assert.True(response2.StatusCode == HttpStatusCode.OK || response2.StatusCode == HttpStatusCode.NotFound);
//        Assert.True(response3.StatusCode == HttpStatusCode.OK || response3.StatusCode == HttpStatusCode.NotFound);
//    }
//}
