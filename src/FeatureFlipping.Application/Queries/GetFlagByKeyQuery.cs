using FeatureFlipping.Application.Common;
using FeatureFlipping.Application.DTOs;
using MediatR;

namespace FeatureFlipping.Application.Queries;

/// <summary>Query to get a feature flag by its key.</summary>
public sealed record GetFlagByKeyQuery(string Key) : IRequest<Result<FeatureFlagDto>>;
