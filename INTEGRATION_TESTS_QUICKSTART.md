# Integration Tests - Quick Start Guide

## Overview
This project now includes comprehensive integration tests for the Weather Forecast API. All tests use real HTTP requests with an in-memory test server.

## Quick Commands

### Run All Tests
```bash
cd WeatherForecast.Api.Tests
dotnet test
```

### Run Specific Test Category
```bash
# Authentication tests only
dotnet test --filter "AuthenticationIntegrationTests"

# Weather service tests only
dotnet test --filter "WeatherServiceIntegrationTests"

# End-to-end tests only
dotnet test --filter "EndToEndIntegrationTests"
```

### Run Specific Test
```bash
dotnet test --filter "Register_WithValidRequest_ReturnsOkWithToken"
```

### Run with Details
```bash
dotnet test --verbosity:normal
```

## Test Structure

```
WeatherForecast.Api.Tests/
??? Fixtures/
?   ??? WeatherForecastWebApplicationFactory.cs    # Test server setup
??? Integration/
?   ??? AuthenticationIntegrationTests.cs          # Registration & login tests
?   ??? WeatherServiceIntegrationTests.cs          # Weather endpoint tests
?   ??? EndToEndIntegrationTests.cs                # Complete user journey tests
??? README.md                                       # Detailed documentation
??? WeatherForecast.Api.Tests.csproj              # Project file
```

## What's Tested

### ? Authentication (13 tests)
- User registration with validation
- User login with credential verification
- Token generation and validation
- Error handling (bad request, conflict, unauthorized)
- Multiple users and unique tokens

### ? Weather Service (10 tests)
- Authorization enforcement
- Protected endpoint access
- Weather data retrieval and validation
- City parameter validation
- Caching behavior
- Error responses

### ? End-to-End (5+ tests)
- Complete user journeys
- Concurrent user operations
- Security boundaries
- Input validation
- Token reusability

## Key Test Scenarios

### Registration
```csharp
[Fact]
public async Task Register_WithValidRequest_ReturnsOkWithToken()
{
    // User can register with valid credentials
    // Returns JWT token
    // Returns user ID and username
}
```

### Login
```csharp
[Fact]
public async Task Login_WithValidCredentials_ReturnsOkWithToken()
{
    // User can login with correct password
    // Returns JWT token
    // Returns same user ID from registration
}
```

### Protected Endpoint
```csharp
[Fact]
public async Task GetWeatherByCity_WithValidToken_IsAccessible()
{
    // Registered user can access weather endpoint with JWT token
    // Weather data is returned
}
```

## Test Patterns Used

### Helper Methods
```csharp
// Register and login a user in one call
private async Task<AuthResponse> RegisterAndLoginUserAsync(string username, string password)

// Create authenticated HTTP client
private HttpClient CreateAuthenticatedClient(string token)
```

### Assertions
```csharp
// Check status code
Assert.Equal(HttpStatusCode.OK, response.StatusCode);

// Check response content
var content = await response.Content.ReadAsAsync<AuthResponse>();
Assert.NotNull(content.Token);

// Check data consistency
Assert.Equal(userId, loginContent.UserId);
```

## Common Test Setup

```csharp
[Fact]
public async Task TestName_Scenario_ExpectedResult()
{
    // Arrange - Setup test data
    var registerRequest = new RegisterRequest 
    { 
        Username = "testuser", 
        Password = "Password123!" 
    };

    // Act - Perform action
    var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

    // Assert - Verify result
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var content = await response.Content.ReadAsAsync<AuthResponse>();
    Assert.NotNull(content.Token);
}
```

## Debugging Tests

### View Test Output
```bash
dotnet test --logger "console;verbosity=detailed"
```

### Run Single Test in Debug
```bash
dotnet test --filter "Register_WithValidRequest_ReturnsOkWithToken" --logger "console;verbosity=detailed"
```

### View Detailed Error Messages
```bash
dotnet test --verbosity:normal
```

## Test Data

Tests use real credentials:
- **Username**: Various test usernames (e.g., "testuser", "user1", "loginuser")
- **Password**: "Password123!" or similar meeting complexity requirements
- **Cities**: "London", "Paris", "Tokyo", "NewYork", "Sydney", "Dubai"

## Important Notes

1. **Isolation**: Each test runs independently - registration state doesn't carry over
2. **In-Memory**: All data is stored in memory, no database persistence
3. **Real HTTP**: Tests use actual HTTP requests, not mocked calls
4. **JWT Tokens**: Real JWT tokens are generated and validated
5. **Deterministic**: Tests always produce consistent results

## Adding New Tests

### Template
```csharp
[Fact]
public async Task FeatureName_Scenario_ExpectedResult()
{
    // Arrange
    var testData = new TestRequest { /* ... */ };

    // Act
    var response = await _client.PostAsJsonAsync("/api/endpoint", testData);

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var content = await response.Content.ReadAsAsync<ResponseType>();
    Assert.NotNull(content);
}
```

### Steps
1. Add test method to appropriate test class
2. Follow naming convention: `MethodName_Scenario_ExpectedResult`
3. Use Arrange-Act-Assert pattern
4. Use helper methods for common operations
5. Run `dotnet test` to verify

## Troubleshooting

### Tests Fail on First Run
- Ensure the API project builds: `dotnet build`
- Check JWT configuration in appsettings.json
- Verify .NET 10 is installed

### HTTP 500 Errors
- Check application logs
- Verify dependency injection setup in Program.cs
- Check for missing configuration sections

### Port Conflicts
- WebApplicationFactory automatically selects available ports
- Not an issue unless running tests simultaneously

## CI/CD Integration

To add tests to CI/CD pipeline:
```yaml
- name: Run Integration Tests
  run: dotnet test WeatherForecast.Api.Tests --verbosity:normal
```

## Coverage Report

To generate code coverage:
```bash
dotnet test /p:CollectCoverageMetrics=true /p:GenerateCoverageReport=true
```

## Performance

- Single test: ~100-500ms
- Full suite (28 tests): ~10-15 seconds
- Tests can run in parallel (Xunit default)

## Next Steps

1. Run tests: `dotnet test`
2. Review test classes for patterns
3. Add more tests as needed
4. Integrate with CI/CD pipeline
5. Monitor coverage metrics

## Questions?

Refer to:
- `WeatherForecast.Api.Tests/README.md` - Detailed documentation
- `INTEGRATION_TESTS_SUMMARY.md` - Complete summary
- Individual test methods - Code examples
