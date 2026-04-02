using FeatureFlipping.Application.Abstractions;
using FeatureFlipping.Domain.Aggregates;
using FeatureFlipping.Domain.Interfaces;
using FeatureFlipping.Domain.ValueObjects;
using FeatureFlipping.Infrastructure.Evaluation;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace FeatureFlipping.Tests.Evaluation;

public sealed class FeatureFlagEvaluatorTests
{
    private readonly IFeatureFlagRepository _repository = Substitute.For<IFeatureFlagRepository>();
    private readonly IFeatureFlagCache _cache = Substitute.For<IFeatureFlagCache>();
    private readonly FeatureFlagEvaluator _evaluator;

    public FeatureFlagEvaluatorTests()
    {
        _evaluator = new FeatureFlagEvaluator(_repository, _cache, NullLogger<FeatureFlagEvaluator>.Instance);
    }

    [Fact]
    public async Task EvaluateAsync_FlagNotFound_ReturnsNotFound()
    {
        var key = new FlagKey("missing-flag");
        _cache.GetAsync(key).Returns(Task.FromResult<FeatureFlag?>(null));
        _repository.GetByKeyAsync(key).Returns(Task.FromResult<FeatureFlag?>(null));

        var result = await _evaluator.EvaluateAsync(key);

        Assert.False(result.IsEnabled);
        Assert.Equal(EvaluationReason.NotFound, result.Reason);
    }

    [Fact]
    public async Task EvaluateAsync_FlagDisabled_ReturnsDisabled()
    {
        var key = new FlagKey("my-flag");
        var flag = FeatureFlag.Create(key, isEnabled: false);
        _cache.GetAsync(key).Returns(Task.FromResult<FeatureFlag?>(flag));

        var result = await _evaluator.EvaluateAsync(key);

        Assert.False(result.IsEnabled);
        Assert.Equal(EvaluationReason.Disabled, result.Reason);
    }

    [Fact]
    public async Task EvaluateAsync_FlagEnabled_NoTargeting_NoRollout_ReturnsEnabled()
    {
        var key = new FlagKey("my-flag");
        var flag = FeatureFlag.Create(key, isEnabled: true, rolloutPercentage: 0);
        _cache.GetAsync(key).Returns(Task.FromResult<FeatureFlag?>(flag));

        var result = await _evaluator.EvaluateAsync(key);

        Assert.True(result.IsEnabled);
        Assert.Equal(EvaluationReason.Enabled, result.Reason);
    }

    [Fact]
    public async Task EvaluateAsync_UserTargeted_ReturnsUserTargeted()
    {
        var key = new FlagKey("my-flag");
        var flag = FeatureFlag.Create(key, isEnabled: true, userTargeting: ["user-42"]);
        _cache.GetAsync(key).Returns(Task.FromResult<FeatureFlag?>(flag));

        var result = await _evaluator.EvaluateAsync(key, "user-42");

        Assert.True(result.IsEnabled);
        Assert.Equal(EvaluationReason.UserTargeted, result.Reason);
    }

    [Fact]
    public async Task EvaluateAsync_Rollout100_ReturnsEnabled()
    {
        var key = new FlagKey("my-flag");
        var flag = FeatureFlag.Create(key, isEnabled: true, rolloutPercentage: 100);
        _cache.GetAsync(key).Returns(Task.FromResult<FeatureFlag?>(flag));

        var result = await _evaluator.EvaluateAsync(key, "any-user");

        Assert.True(result.IsEnabled);
        Assert.Equal(EvaluationReason.Enabled, result.Reason);
    }

    [Fact]
    public async Task EvaluateAsync_RolloutDeterminesResult_IsDeterministic()
    {
        var key = new FlagKey("rollout-flag");
        var flag = FeatureFlag.Create(key, isEnabled: true, rolloutPercentage: 50);
        _cache.GetAsync(key).Returns(Task.FromResult<FeatureFlag?>(flag));

        var result1 = await _evaluator.EvaluateAsync(key, "user-deterministic");
        var result2 = await _evaluator.EvaluateAsync(key, "user-deterministic");

        Assert.Equal(result1.IsEnabled, result2.IsEnabled);
        Assert.Equal(result1.Reason, result2.Reason);
    }

    [Fact]
    public async Task EvaluateAsync_CacheMiss_LoadsFromRepository()
    {
        var key = new FlagKey("uncached-flag");
        var flag = FeatureFlag.Create(key, isEnabled: true);
        _cache.GetAsync(key).Returns(Task.FromResult<FeatureFlag?>(null));
        _repository.GetByKeyAsync(key).Returns(Task.FromResult<FeatureFlag?>(flag));

        var result = await _evaluator.EvaluateAsync(key);

        Assert.True(result.IsEnabled);
        await _cache.Received(1).SetAsync(key, flag);
    }

    [Fact]
    public async Task EvaluateAsync_UserTargeted_WhenFlagGloballyDisabled_ReturnsUserTargeted()
    {
        var key = new FlagKey("my-flag");
        // Flag is globally off but user-42 is explicitly targeted
        var flag = FeatureFlag.Create(key, isEnabled: false, userTargeting: ["user-42"]);
        _cache.GetAsync(key).Returns(Task.FromResult<FeatureFlag?>(flag));

        var result = await _evaluator.EvaluateAsync(key, "user-42");

        Assert.True(result.IsEnabled);
        Assert.Equal(EvaluationReason.UserTargeted, result.Reason);
    }

    [Fact]
    public async Task EvaluateAsync_NonTargetedUser_WhenFlagGloballyDisabled_ReturnsDisabled()
    {
        var key = new FlagKey("my-flag");
        var flag = FeatureFlag.Create(key, isEnabled: false, userTargeting: ["user-42"]);
        _cache.GetAsync(key).Returns(Task.FromResult<FeatureFlag?>(flag));

        var result = await _evaluator.EvaluateAsync(key, "user-99");

        Assert.False(result.IsEnabled);
        Assert.Equal(EvaluationReason.Disabled, result.Reason);
    }
}
