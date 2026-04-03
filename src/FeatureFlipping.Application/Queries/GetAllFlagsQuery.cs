using FeatureFlipping.Application.Common;
using FeatureFlipping.Application.DTOs;
using MediatR;

namespace FeatureFlipping.Application.Queries;

/// <summary>Query to get all feature flags.</summary>
public sealed record GetAllFlagsQuery : IRequest<Result<IReadOnlyList<FeatureFlagDto>>>;
