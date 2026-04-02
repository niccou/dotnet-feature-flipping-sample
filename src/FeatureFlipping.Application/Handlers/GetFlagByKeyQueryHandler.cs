using FeatureFlipping.Application.Common;
using FeatureFlipping.Application.DTOs;
using FeatureFlipping.Application.Queries;
using FeatureFlipping.Domain.Interfaces;
using FeatureFlipping.Domain.ValueObjects;
using MediatR;

namespace FeatureFlipping.Application.Handlers;

/// <summary>Handles the GetFlagByKeyQuery.</summary>
public sealed class GetFlagByKeyQueryHandler : IRequestHandler<GetFlagByKeyQuery, Result<FeatureFlagDto>>
{
    private readonly IFeatureFlagRepository _repository;

    /// <summary>Initializes the handler.</summary>
    public GetFlagByKeyQueryHandler(IFeatureFlagRepository repository)
    {
        _repository = repository;
    }

    /// <inheritdoc/>
    public async Task<Result<FeatureFlagDto>> Handle(GetFlagByKeyQuery request, CancellationToken cancellationToken)
    {
        FlagKey key;
        try { key = new FlagKey(request.Key); }
        catch { return Result<FeatureFlagDto>.Failure($"Invalid flag key: {request.Key}"); }

        var flag = await _repository.GetByKeyAsync(key, cancellationToken);
        if (flag is null)
            return Result<FeatureFlagDto>.Failure($"Flag '{request.Key}' not found.");

        return Result<FeatureFlagDto>.Success(new FeatureFlagDto(
            flag.Key, flag.IsEnabled, flag.Value, flag.GetUserTargeting(), flag.RolloutPercentage, flag.UpdatedAt
        ));
    }
}
