using FeatureFlipping.Application.Abstractions;
using FeatureFlipping.Domain.Interfaces;
using FeatureFlipping.Infrastructure.Caching;
using FeatureFlipping.Infrastructure.Evaluation;
using FeatureFlipping.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FeatureFlipping.Infrastructure;

/// <summary>DI registration for the infrastructure layer.</summary>
public static class DependencyInjection
{
    /// <summary>Registers all infrastructure services.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<FeatureFlagDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddMemoryCache();
        services.AddScoped<IFeatureFlagRepository, FeatureFlagRepository>();
        services.AddScoped<IFeatureFlagEvaluator, FeatureFlagEvaluator>();
        services.AddSingleton<IFeatureFlagCache, InMemoryFeatureFlagCache>();

        return services;
    }
}
