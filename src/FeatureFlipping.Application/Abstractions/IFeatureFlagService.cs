using FeatureFlipping.Application.Common;
using FeatureFlipping.Application.DTOs;

namespace FeatureFlipping.Application.Abstractions;

/// <summary>Service abstraction for CRUD + toggle operations on feature flags.</summary>
public interface IFeatureFlagService
{
    /// <summary>Gets all feature flags.</summary>
    Task<Result<IReadOnlyList<FeatureFlagDto>>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Gets a feature flag by key.</summary>
    Task<Result<FeatureFlagDto>> GetByKeyAsync(string key, CancellationToken ct = default);

    /// <summary>Creates a new feature flag.</summary>
    Task<Result<FeatureFlagDto>> CreateAsync(CreateFeatureFlagDto dto, CancellationToken ct = default);

    /// <summary>Updates an existing feature flag.</summary>
    Task<Result<FeatureFlagDto>> UpdateAsync(string key, UpdateFeatureFlagDto dto, CancellationToken ct = default);

    /// <summary>Toggles a feature flag.</summary>
    Task<Result<FeatureFlagDto>> ToggleAsync(string key, CancellationToken ct = default);
}
