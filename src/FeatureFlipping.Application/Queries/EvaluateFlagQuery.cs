using FeatureFlipping.Application.Common;
using FeatureFlipping.Application.DTOs;
using MediatR;

namespace FeatureFlipping.Application.Queries;

/// <summary>Query to evaluate a feature flag for a user.</summary>
public sealed record EvaluateFlagQuery(string Key, string? UserId) : IRequest<Result<FlagEvaluationResultDto>>;
