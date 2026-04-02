using FeatureFlipping.Application.Common;
using FeatureFlipping.Application.DTOs;
using FeatureFlipping.Application.Queries;
using FeatureFlipping.Domain.Interfaces;
using MediatR;

namespace FeatureFlipping.Application.Handlers;

/// <summary>Handles the GetAllFlagsQuery.</summary>
public sealed class GetAllFlagsQueryHandler : IRequestHandler<GetAllFlagsQuery, Result<IReadOnlyList<FeatureFlagDto>>>
{
    private readonly IFeatureFlagRepository _repository;

    /// <summary>Initializes the handler.</summary>
    public GetAllFlagsQueryHandler(IFeatureFlagRepository repository)
    {
        _repository = repository;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<FeatureFlagDto>>> Handle(GetAllFlagsQuery request, CancellationToken cancellationToken)
    {
        var flags = await _repository.GetAllAsync(cancellationToken);
        var dtos = flags.Select(f => new FeatureFlagDto(
            f.Key, f.IsEnabled, f.Value, f.GetUserTargeting(), f.RolloutPercentage, f.UpdatedAt
        )).ToList();
        return Result<IReadOnlyList<FeatureFlagDto>>.Success(dtos);
    }
}
