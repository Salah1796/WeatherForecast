# Weather Forecast API

A Clean Architecture-based REST API for checking weather forecasts, built with .NET and keeping SOLID principles in mind.

## Features
- **Authentication**: JWT-based login and registration.
- **Weather Data**: Retrieve weather information by city (secured endpoint).
- **Caching**: InMemory caching using Decorator pattern to optimize weather retrieval.
- **Architecture**: Follows Clean Architecture (API, Application, Domain, Infrastructure).
- **Localization**: Supports English (en) and Arabic (ar).
- **Tests**: Comprehensive Unit and Integration tests.

## Getting Started

### Prerequisites
- .NET SDK (matches the project version, e.g., .NET 10.0 or latest preview)
- Git

### Installation
1. Clone the repository:
   ```bash
   git clone https://github.com/Salah1796/WeatherForecast.git   
   ```
2. Navigate to the solution folder:
   ```bash
   cd WeatherForecast
   ```

### Running the API
1. Restore dependencies:
   ```bash
   dotnet restore
   ```
2. Run the API:
   ```bash
   dotnet run --project WeatherForecast.Api
   ```
3. The API will start on `https://localhost:7152` (or configured port).
4. Swagger UI is available at `https://localhost:7152/index.html` (in Development).

### Running Tests
To run all unit and integration tests:
```bash
dotnet test
```

## Architecture

The solution maps to the following layers:
- **WeatherForecast.Api**: Entry point, Controllers, Middleware, IoC configuration.
- **WeatherForecast.Application**: Service interfaces, DTOs, Business Logic (Services), Validators.
- **WeatherForecast.Domain**: Entities, Value Objects, Repository Interfaces.
- **WeatherForecast.Infrastructure**: Implementation of Repositories, External Services (Mock), and Caching decorators.

## Design Patterns Used
- **Repository Pattern**: Abstraction over data access.
- **Decorator Pattern**: Used for `CachedWeatherService` to add caching behavior without modifying valid business logic.
- **Result Pattern**: Standardized response wrapper for success/failure scenarios.
