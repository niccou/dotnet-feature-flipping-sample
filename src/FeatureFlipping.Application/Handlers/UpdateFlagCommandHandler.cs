using FeatureFlipping.Application.Abstractions;
using FeatureFlipping.Application.Commands;
using FeatureFlipping.Application.Common;
using FeatureFlipping.Application.DTOs;
using FeatureFlipping.Domain.Interfaces;
using FeatureFlipping.Domain.ValueObjects;
using MediatR;

namespace FeatureFlipping.Application.Handlers;

/// <summary>Handles the UpdateFlagCommand.</summary>
public sealed class UpdateFlagCommandHandler : IRequestHandler<UpdateFlagCommand, Result<FeatureFlagDto>>
{
    private readonly IFeatureFlagRepository _repository;
    private readonly IFeatureFlagCache _cache;

    /// <summary>Initializes the handler.</summary>
    public UpdateFlagCommandHandler(IFeatureFlagRepository repository, IFeatureFlagCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    /// <inheritdoc/>
    public async Task<Result<FeatureFlagDto>> Handle(UpdateFlagCommand request, CancellationToken cancellationToken)
    {
        FlagKey key;
        try { key = new FlagKey(request.Key); }
        catch { return Result<FeatureFlagDto>.Failure($"Invalid flag key: {request.Key}"); }

        var flag = await _repository.GetByKeyAsync(key, cancellationToken);
        if (flag is null)
            return Result<FeatureFlagDto>.Failure($"Flag '{request.Key}' not found.");

        flag.Update(request.Dto.IsEnabled, request.Dto.Value, request.Dto.UserTargeting, request.Dto.RolloutPercentage);
        await _repository.UpdateAsync(flag, cancellationToken);
        await _cache.InvalidateAsync(key);

        return Result<FeatureFlagDto>.Success(new FeatureFlagDto(
            flag.Key, flag.IsEnabled, flag.Value, flag.GetUserTargeting(), flag.RolloutPercentage, flag.UpdatedAt
        ));
    }
}
