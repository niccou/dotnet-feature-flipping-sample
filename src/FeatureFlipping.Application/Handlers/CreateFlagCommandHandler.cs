using FeatureFlipping.Application.Commands;
using FeatureFlipping.Application.Common;
using FeatureFlipping.Application.DTOs;
using FeatureFlipping.Domain.Aggregates;
using FeatureFlipping.Domain.Interfaces;
using FeatureFlipping.Domain.ValueObjects;
using MediatR;

namespace FeatureFlipping.Application.Handlers;

/// <summary>Handles the CreateFlagCommand.</summary>
public sealed class CreateFlagCommandHandler : IRequestHandler<CreateFlagCommand, Result<FeatureFlagDto>>
{
    private readonly IFeatureFlagRepository _repository;

    /// <summary>Initializes the handler.</summary>
    public CreateFlagCommandHandler(IFeatureFlagRepository repository)
    {
        _repository = repository;
    }

    /// <inheritdoc/>
    public async Task<Result<FeatureFlagDto>> Handle(CreateFlagCommand request, CancellationToken cancellationToken)
    {
        FlagKey key;
        try { key = new FlagKey(request.Dto.Key); }
        catch (Exception ex) { return Result<FeatureFlagDto>.Failure(ex.Message); }

        if (await _repository.ExistsAsync(key, cancellationToken))
            return Result<FeatureFlagDto>.Failure($"Flag '{request.Dto.Key}' already exists.");

        var flag = FeatureFlag.Create(key, request.Dto.IsEnabled, request.Dto.Value,
            request.Dto.UserTargeting, request.Dto.RolloutPercentage);
        await _repository.AddAsync(flag, cancellationToken);

        return Result<FeatureFlagDto>.Success(new FeatureFlagDto(
            flag.Key, flag.IsEnabled, flag.Value, flag.GetUserTargeting(), flag.RolloutPercentage, flag.UpdatedAt
        ));
    }
}
