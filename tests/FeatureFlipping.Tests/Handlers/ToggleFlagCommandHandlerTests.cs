using FeatureFlipping.Application.Abstractions;
using FeatureFlipping.Application.Commands;
using FeatureFlipping.Application.Handlers;
using FeatureFlipping.Domain.Aggregates;
using FeatureFlipping.Domain.Interfaces;
using FeatureFlipping.Domain.ValueObjects;
using NSubstitute;
using Xunit;

namespace FeatureFlipping.Tests.Handlers;

public sealed class ToggleFlagCommandHandlerTests
{
    private readonly IFeatureFlagRepository _repository = Substitute.For<IFeatureFlagRepository>();
    private readonly IFeatureFlagCache _cache = Substitute.For<IFeatureFlagCache>();
    private readonly ToggleFlagCommandHandler _handler;

    public ToggleFlagCommandHandlerTests()
    {
        _handler = new ToggleFlagCommandHandler(_repository, _cache);
    }

    [Fact]
    public async Task Handle_ValidKey_TogglesFlag_AndInvalidatesCache()
    {
        var key = new FlagKey("my-flag");
        var flag = FeatureFlag.Create(key, isEnabled: false);
        _repository.GetByKeyAsync(Arg.Any<FlagKey>()).Returns(Task.FromResult<FeatureFlag?>(flag));

        var result = await _handler.Handle(new ToggleFlagCommand("my-flag"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.IsEnabled);
        await _cache.Received(1).InvalidateAsync(Arg.Any<FlagKey>());
    }

    [Fact]
    public async Task Handle_FlagNotFound_ReturnsFailure()
    {
        _repository.GetByKeyAsync(Arg.Any<FlagKey>()).Returns(Task.FromResult<FeatureFlag?>(null));

        var result = await _handler.Handle(new ToggleFlagCommand("missing-flag"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.Error);
        await _cache.DidNotReceive().InvalidateAsync(Arg.Any<FlagKey>());
    }

    [Fact]
    public async Task Handle_InvalidKey_ReturnsFailure()
    {
        var result = await _handler.Handle(new ToggleFlagCommand("INVALID KEY!"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        await _repository.DidNotReceive().GetByKeyAsync(Arg.Any<FlagKey>());
    }

    [Fact]
    public async Task Handle_ToggleEnabledFlag_DisablesIt()
    {
        var key = new FlagKey("enabled-flag");
        var flag = FeatureFlag.Create(key, isEnabled: true);
        _repository.GetByKeyAsync(Arg.Any<FlagKey>()).Returns(Task.FromResult<FeatureFlag?>(flag));

        var result = await _handler.Handle(new ToggleFlagCommand("enabled-flag"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value!.IsEnabled);
    }
}
