using FeatureFlipping.Application.Common;
using FeatureFlipping.Application.DTOs;
using FeatureFlipping.Application.Queries;
using FeatureFlipping.Domain.Interfaces;
using FeatureFlipping.Domain.ValueObjects;
using MediatR;

namespace FeatureFlipping.Application.Handlers;

/// <summary>Handles the EvaluateFlagQuery.</summary>
public sealed class EvaluateFlagQueryHandler : IRequestHandler<EvaluateFlagQuery, Result<FlagEvaluationResultDto>>
{
    private readonly IFeatureFlagEvaluator _evaluator;

    /// <summary>Initializes the handler.</summary>
    public EvaluateFlagQueryHandler(IFeatureFlagEvaluator evaluator)
    {
        _evaluator = evaluator;
    }

    /// <inheritdoc/>
    public async Task<Result<FlagEvaluationResultDto>> Handle(EvaluateFlagQuery request, CancellationToken cancellationToken)
    {
        FlagKey key;
        try { key = new FlagKey(request.Key); }
        catch { return Result<FlagEvaluationResultDto>.Failure($"Invalid flag key: {request.Key}"); }

        var result = await _evaluator.EvaluateAsync(key, request.UserId, cancellationToken);
        return Result<FlagEvaluationResultDto>.Success(new FlagEvaluationResultDto(
            request.Key, result.IsEnabled, result.Value, result.EvaluatedAt, result.Reason.ToString()
        ));
    }
}
