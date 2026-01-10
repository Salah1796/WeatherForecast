using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;
using WeatherForecast.Application.Common.Localization;
using WeatherForecast.Application.Interfaces;
using WeatherForecast.Domain.Repositories;
using WeatherForecast.Infrastructure.Data;
using WeatherForecast.Infrastructure.Repositories;
using WeatherForecast.Infrastructure.Services;

namespace WeatherForecast.Infrastructure;

/// <summary>
/// Extension methods for dependency injection configuration.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds infrastructure services to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register DB Context
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Register memory cache
        services.AddMemoryCache();

        // Register repositories
        services.AddScoped<IUserRepository, EfUserRepository>();
        services.AddSingleton<IWeatherRepository, MockWeatherRepository>();

        // Register services
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenGenerator, JwtTokenGenerator>();

        // Register cached weather service decorator
        // Note: The base WeatherService from Application layer will be registered by AddApplication
        // We wrap it with caching here using Scrutor's Decorate method
        services.Decorate<IWeatherService>((inner, sp) =>
        {
            var cache = sp.GetRequiredService<IMemoryCache>();
            var iappLocalizer = sp.GetRequiredService<IAppLocalizer>();
            return new CachedWeatherService(inner, cache, configuration, iappLocalizer);
        });

        return services;
    }
}

