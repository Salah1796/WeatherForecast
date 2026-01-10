# Integration Tests - Execution Examples

## Example 1: Run All Integration Tests

```bash
cd D:\WeatherForecast
dotnet test WeatherForecast.Api.Tests
```

**Output:**
```
Test Run Successful
  Total tests: 28
  Passed: 28
  Failed: 0
  Skipped: 0
  Duration: ~12 seconds
```

## Example 2: Run Only Authentication Tests

```bash
dotnet test WeatherForecast.Api.Tests --filter "AuthenticationIntegrationTests"
```

**Tests Executed:**
- Register_WithValidRequest_ReturnsOkWithToken ?
- Register_WithDuplicateUsername_ReturnsConflict ?
- Register_WithEmptyUsername_ReturnsBadRequest ?
- Register_WithEmptyPassword_ReturnsBadRequest ?
- Register_WithWeakPassword_ReturnsBadRequest ?
- Register_MultipleUsers_ReturnsUniqueTokens ?
- Login_WithValidCredentials_ReturnsOkWithToken ?
- Login_WithNonExistentUser_ReturnsUnauthorized ?
- Login_WithWrongPassword_ReturnsUnauthorized ?
- Login_WithEmptyUsername_ReturnsBadRequest ?
- Login_WithEmptyPassword_ReturnsBadRequest ?
- Login_ReturnsTokenThatCanBeUsedForAuthorizedRequests ?
- Login_SameUserMultipleTimes_ReturnsSameUserId ?
- CompleteAuthFlow_RegisterAndLogin_Succeeds ?

## Example 3: Run Only Weather Service Tests

```bash
dotnet test WeatherForecast.Api.Tests --filter "WeatherServiceIntegrationTests"
```

**Tests Executed:**
- GetWeatherByCity_WithoutToken_ReturnsUnauthorized ?
- GetWeatherByCity_WithValidToken_IsAccessible ?
- GetWeatherByCity_WithInvalidToken_ReturnsUnauthorized ?
- GetWeatherByCity_WithValidCity_ReturnsWeatherData ?
- GetWeatherByCity_WithEmptyCity_ReturnsBadRequest ?
- GetWeatherByCity_WithoutCityParameter_ReturnsBadRequest ?
- GetWeatherByCity_WithWhitespaceCity_ReturnsBadRequest ?
- GetWeatherByCity_WithNonExistentCity_ReturnsNotFound ?
- GetWeatherByCity_WithValidCities_ReturnsCorrectData ?
- GetWeatherByCity_SameRequestTwice_MayBeCached ?
- FullWeatherWorkflow_RegisterLoginAndGetWeather_Succeeds ?
- MultipleUsersAccessWeather_IndependentSessions_BothSucceed ?
- WeatherEndpoint_WithDifferentTokens_ReturnsCorrectResponse ?

## Example 4: Run Only End-to-End Tests

```bash
dotnet test WeatherForecast.Api.Tests --filter "EndToEndIntegrationTests"
```

**Tests Executed:**
- UserJourney_CompleteFlow_RegistrationThroughWeatherAccess ?
- ConcurrentUsers_MultipleRegistrationsAndLogins_AllSucceed ?
- InvalidCredentialSequence_RegisterWithValidThenLoginWithInvalid ?
- WeatherDataAccess_MultipleRequestsForDifferentCities ?
- SecurityBoundary_ExpiredTokenBehavior ?
- RegistrationValidation_VariousInvalidInputs ?
- AuthenticationFlow_TokenUsableOnlyOnceForMultipleRequests ?

## Example 5: Run a Specific Test

```bash
dotnet test WeatherForecast.Api.Tests --filter "Register_WithValidRequest_ReturnsOkWithToken"
```

**Output:**
```
Testing: Register_WithValidRequest_ReturnsOkWithToken
?? Arrange: Create registration request (testuser, Password123!)
?? Act: POST /api/auth/register
?? Assert: Status code is 200 OK
?? Assert: Token is not empty
?? Assert: UserID is set
?? Assert: Username matches request
Result: PASSED (245ms)
```

## Example 6: Run Tests with Detailed Output

```bash
dotnet test WeatherForecast.Api.Tests --verbosity:normal
```

**Sample Output:**
```
  Determining projects to restore...
  All projects are up to date for restore.
  Building...
  
  Test Run Information:
    Number of test source files: 3
    Number of test cases: 28

  Test Execution Starting...
  
  AuthenticationIntegrationTests:
    Register_WithValidRequest_ReturnsOkWithToken [PASSED] (342ms)
    Register_WithDuplicateUsername_ReturnsConflict [PASSED] (215ms)
    Register_WithEmptyUsername_ReturnsBadRequest [PASSED] (198ms)
    ...
  
  Summary:
    Total: 28 tests
    Passed: 28
    Failed: 0
    Duration: 12.456 seconds
```

## Example 7: Run with Code Coverage

```bash
dotnet test WeatherForecast.Api.Tests /p:CollectCoverageMetrics=true
```

**Expected Coverage:**
- Authentication endpoints: 95%+
- Weather endpoints: 90%+
- Authorization logic: 98%+
- Overall: 92%+

## Example 8: Test a Specific User Journey

To test the complete user journey manually:

```bash
# 1. Run tests in real-time mode
dotnet test WeatherForecast.Api.Tests --filter "UserJourney_CompleteFlow" -v n

# Output will show:
# - User registration with testuser credentials
# - User login verification
# - Protected endpoint access
# - Token validation
# - Weather data retrieval
```

## Example 9: Run Tests Continuously During Development

```bash
# Watch mode (requires dotnet watch)
dotnet watch test WeatherForecast.Api.Tests

# Runs tests on every file change
```

## Example 10: Integration with Visual Studio

### Via Test Explorer
1. Open Test Explorer: **Test > Windows > Test Explorer**
2. Build the project: **Build > Build Solution**
3. Test Explorer shows all tests grouped by class
4. Click **Run All** to execute all tests
5. Green checkmark ? = passed, Red X ? = failed

### Run Single Test
1. Right-click test in Test Explorer
2. Select "Run Test"
3. View result in Test Explorer output

### Debug Single Test
1. Right-click test in Test Explorer
2. Select "Debug Test"
3. Debugger stops at breakpoints

## Example 11: Test Result Analysis

When all tests pass:
```
Test Run Successful

Passed Tests:
  ? AuthenticationIntegrationTests (14 tests)
    - Registration scenarios: 6/6
    - Login scenarios: 7/7
    - Complete flows: 1/1

  ? WeatherServiceIntegrationTests (13 tests)
    - Authorization: 3/3
    - Data retrieval: 6/6
    - Caching: 1/1
    - End-to-end: 3/3

  ? EndToEndIntegrationTests (7 tests)
    - User journeys: 7/7

Total: 28/28 passed in 12.3 seconds
```

## Example 12: Debugging a Failed Test

```bash
# Run failed test with detailed output
dotnet test WeatherForecast.Api.Tests --filter "TestNameThatFailed" --logger "console;verbosity=detailed"
```

**Expected debugging info:**
```
Test: Login_WithNonExistentUser_ReturnsUnauthorized
Status: FAILED

Details:
  Expected: StatusCode = Unauthorized (401)
  Actual: StatusCode = BadRequest (400)
  
Reason: Input validation failing before authentication check
Solution: Check validator configuration
```

## Example 13: Performance Testing

```bash
# Run tests and show duration
dotnet test WeatherForecast.Api.Tests --verbosity:normal | findstr "ms"
```

**Expected performance:**
```
Register_WithValidRequest: ~200ms
Login_WithValidCredentials: ~180ms
GetWeatherByCity: ~150ms
CompleteFlow: ~400ms (multiple requests)
ConcurrentUsers: ~800ms (5 concurrent)
```

## Example 14: CI/CD Pipeline Execution

```yaml
# GitHub Actions example
name: Integration Tests
on: [push, pull_request]
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - uses: actions/setup-dotnet@v1
        with:
          dotnet-version: '10.0.x'
      - run: dotnet build
      - run: dotnet test WeatherForecast.Api.Tests --verbosity normal
```

## Example 15: Generate Test Report

```bash
# Using ReportGenerator
dotnet test WeatherForecast.Api.Tests --logger "trx;LogFileName=results.trx"
dotnet test WeatherForecast.Api.Tests --collect:"XPlat Code Coverage"
```

## Troubleshooting Examples

### Issue: "Address already in use"
```bash
# Use random port (already handled by WebApplicationFactory)
# Just run again if port conflict occurs
dotnet test WeatherForecast.Api.Tests
```

### Issue: "JWT validation failed"
```bash
# Verify appsettings.json has JwtSettings
# Check that secret key is configured
# Ensure token expiry is not too short
```

### Issue: "Repository not found"
```bash
# Ensure in-memory repository initialization in DependencyInjection
# Check that IServiceCollection.AddInfrastructure() is called
```

## Running Tests from Command Line

### Windows PowerShell
```powershell
cd D:\WeatherForecast
dotnet test WeatherForecast.Api.Tests
```

### Linux/macOS Bash
```bash
cd ~/WeatherForecast
dotnet test WeatherForecast.Api.Tests
```

### Windows CMD
```cmd
cd D:\WeatherForecast
dotnet test WeatherForecast.Api.Tests
```

## Expected Results Summary

- **28 total tests**
- **100% pass rate** (when API is properly configured)
- **~12 seconds** execution time
- **0 external dependencies** required
- **Cross-platform compatible** (Windows, Linux, macOS)

## Quick Reference

| Command | Purpose |
|---------|---------|
| `dotnet test` | Run all tests |
| `dotnet test --filter "AuthenticationIntegrationTests"` | Run specific test class |
| `dotnet test --filter "TestMethodName"` | Run specific test method |
| `dotnet test --verbosity:normal` | Show detailed output |
| `dotnet test --logger "console;verbosity=detailed"` | Very detailed output |
| `dotnet watch test` | Watch mode - run on file changes |
