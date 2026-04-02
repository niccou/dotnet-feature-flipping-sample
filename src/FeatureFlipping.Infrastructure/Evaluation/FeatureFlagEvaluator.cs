using System.Security.Cryptography;
using System.Text;
using FeatureFlipping.Application.Abstractions;
using FeatureFlipping.Domain.Interfaces;
using FeatureFlipping.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace FeatureFlipping.Infrastructure.Evaluation;

/// <summary>Evaluates feature flags using cache-first strategy.</summary>
public sealed class FeatureFlagEvaluator : IFeatureFlagEvaluator
{
    private readonly IFeatureFlagRepository _repository;
    private readonly IFeatureFlagCache _cache;
    private readonly ILogger<FeatureFlagEvaluator> _logger;

    /// <summary>Initializes the evaluator.</summary>
    public FeatureFlagEvaluator(IFeatureFlagRepository repository, IFeatureFlagCache cache, ILogger<FeatureFlagEvaluator> logger)
    {
        _repository = repository;
        _cache = cache;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<FlagEvaluationResult> EvaluateAsync(FlagKey key, string? userId = null, CancellationToken ct = default)
    {
        var flag = await _cache.GetAsync(key);
        if (flag is null)
        {
            flag = await _repository.GetByKeyAsync(key, ct);
            if (flag is null)
            {
                _logger.LogDebug("Flag '{Key}' not found", key.Value);
                return FlagEvaluationResult.NotFound();
            }
            await _cache.SetAsync(key, flag);
        }

        if (!flag.IsEnabled)
        {
            _logger.LogDebug("Flag '{Key}' is disabled", key.Value);
            return new FlagEvaluationResult(false, flag.Value, DateTime.UtcNow, EvaluationReason.Disabled);
        }

        var targeting = flag.GetUserTargeting();
        if (targeting.Length > 0 && userId is not null && targeting.Contains(userId))
        {
            _logger.LogDebug("Flag '{Key}' enabled for user '{UserId}' via targeting", key.Value, userId);
            return new FlagEvaluationResult(true, flag.Value, DateTime.UtcNow, EvaluationReason.UserTargeted);
        }

        if (flag.RolloutPercentage > 0 && flag.RolloutPercentage < 100 && userId is not null)
        {
            var hash = ComputeRolloutHash(userId, key.Value);
            if (hash < flag.RolloutPercentage)
            {
                _logger.LogDebug("Flag '{Key}' enabled for user '{UserId}' via {Pct}% rollout (hash={Hash})", key.Value, userId, flag.RolloutPercentage, hash);
                return new FlagEvaluationResult(true, flag.Value, DateTime.UtcNow, EvaluationReason.RolledOut);
            }
            else
            {
                _logger.LogDebug("Flag '{Key}' disabled for user '{UserId}' via {Pct}% rollout (hash={Hash})", key.Value, userId, flag.RolloutPercentage, hash);
                return new FlagEvaluationResult(false, flag.Value, DateTime.UtcNow, EvaluationReason.Disabled);
            }
        }

        _logger.LogDebug("Flag '{Key}' enabled (full rollout or no targeting)", key.Value);
        return new FlagEvaluationResult(true, flag.Value, DateTime.UtcNow, EvaluationReason.Enabled);
    }

    private static int ComputeRolloutHash(string userId, string flagKey)
    {
        var input = Encoding.UTF8.GetBytes($"{userId}{flagKey}");
        var hash = SHA256.HashData(input);
        return (int)(BitConverter.ToUInt32(hash, 0) % 100);
    }
}
