# Weather Forecast API - Integration Tests

This project contains comprehensive integration tests for the Weather Forecast API, covering registration, login, and weather service functionality.

## Overview

The integration tests are located in `WeatherForecast.Api.Tests` and are organized into three main test classes:

### 1. AuthenticationIntegrationTests
Tests the complete authentication flow including registration and login endpoints.

**Key Test Scenarios:**
- **Registration Tests:**
  - Valid registration returns token and user data
  - Duplicate username registration returns conflict error
  - Empty username/password returns bad request
  - Weak password validation
  - Multiple users receive unique tokens

- **Login Tests:**
  - Valid credentials return token and user data
  - Non-existent user returns unauthorized
  - Wrong password returns unauthorized
  - Empty credentials return bad request
  - Same user login returns same user ID
  - Login token can be used for authorized requests

### 2. WeatherServiceIntegrationTests
Tests the weather endpoint functionality with proper authorization and data retrieval.

**Key Test Scenarios:**
- **Authorization Tests:**
  - Endpoint requires valid JWT token
  - Invalid tokens are rejected
  - Valid tokens grant access

- **Weather Data Retrieval:**
  - Valid city returns weather data
  - Empty/whitespace city returns bad request
  - Non-existent city returns not found
  - Multiple cities return correct data
  - Response data is valid (temperature ranges, condition text)

- **Caching Tests:**
  - Repeated requests for same city return consistent data
  - Caching mechanism works transparently

### 3. EndToEndIntegrationTests
Tests complete user journeys and system-wide integration scenarios.

**Key Test Scenarios:**
- **User Journey:** Complete flow from registration ? login ? weather access
- **Concurrent Users:** Multiple users can register and login simultaneously
- **Invalid Credentials:** Wrong password handling and recovery
- **Multi-City Access:** Accessing weather data for different cities
- **Security:** Token validation and invalid token rejection
- **Input Validation:** Various invalid registration inputs
- **Token Reusability:** Same token works for multiple requests

## Test Architecture

### WebApplicationFactory

The `WeatherForecastWebApplicationFactory` provides:
- A configured test HTTP client
- Proper dependency injection setup matching production
- Configuration overrides for testing
- Clean isolation between tests (in-memory repositories)

### Test Fixture Usage

All integration tests use the `WeatherForecastWebApplicationFactory` class fixture to ensure:
- Proper service initialization
- Clean state for each test
- Real HTTP client interactions
- Full middleware pipeline execution

## Running the Tests

### Run all integration tests:
```bash
dotnet test WeatherForecast.Api.Tests
```

### Run specific test class:
```bash
dotnet test WeatherForecast.Api.Tests --filter "AuthenticationIntegrationTests"
```

### Run specific test:
```bash
dotnet test WeatherForecast.Api.Tests --filter "FullName=WeatherForecast.Api.Tests.Integration.AuthenticationIntegrationTests.Register_WithValidRequest_ReturnsOkWithToken"
```

### Run with verbose output:
```bash
dotnet test WeatherForecast.Api.Tests --verbosity:normal
```

## Test Coverage

The integration tests cover:
- ? User registration with validation
- ? User login with credential verification
- ? JWT token generation and usage
- ? Authorization enforcement on protected endpoints
- ? Weather data retrieval for authorized users
- ? Error handling (400, 401, 404, 409 status codes)
- ? Concurrent user operations
- ? Data consistency across operations
- ? Token reusability across requests
- ? Security boundaries

## Key Features

### Isolation
- Each test runs independently with in-memory repositories
- No data persistence between tests
- No external dependencies required

### Comprehensiveness
- Happy path scenarios
- Error handling and edge cases
- Security and authorization
- Concurrent operations
- End-to-end user journeys

### Maintainability
- Clear test names describing what is being tested
- Helper methods for common operations
- Well-organized test sections
- Meaningful assertions with clear failure messages

## Dependencies

The integration tests use:
- **Xunit**: Testing framework
- **Microsoft.AspNetCore.Mvc.Testing**: Integration test utilities
- **System.Net.Http.Json**: JSON serialization for HTTP requests/responses

## Mock Data

The integration tests use:
- **In-Memory User Repository**: Stores registered users during tests
- **Mock Weather Repository**: Provides predefined weather data for cities

## Notes

1. **Token Format**: JWT tokens are generated with claims including user ID and username
2. **Password Hashing**: Passwords are hashed using BCrypt before storage
3. **Authorization**: Uses JWT Bearer tokens in Authorization header
4. **Caching**: Weather data may be cached (transparent to tests)
5. **Localization**: Supports English and Arabic messages (not tested in integration tests)

## Extending the Tests

To add more integration tests:

1. Create a new test method in appropriate test class
2. Use the `_factory` and `_client` fields for HTTP interactions
3. Use helper methods like `RegisterAndLoginUserAsync()` for common operations
4. Follow naming convention: `MethodUnderTest_Scenario_ExpectedResult`
5. Use appropriate `HttpStatusCode` assertions

Example:
```csharp
[Fact]
public async Task GetWeatherByCity_WithSpecificFormat_ReturnsFormattedResponse()
{
    // Arrange
    var authResponse = await RegisterAndLoginUserAsync();
    var authenticatedClient = CreateAuthenticatedClient(authResponse.Token);

    // Act
    var response = await authenticatedClient.GetAsync("/api/weather?city=London");
    var content = await response.Content.ReadAsAsync<WeatherResponse>();

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.NotNull(content);
    // Add more specific assertions
}
```
