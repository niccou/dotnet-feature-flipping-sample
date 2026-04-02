using FeatureFlipping.Application.Common;
using FeatureFlipping.Application.DTOs;
using MediatR;

namespace FeatureFlipping.Application.Commands;

/// <summary>Command to create a new feature flag.</summary>
public sealed record CreateFlagCommand(CreateFeatureFlagDto Dto) : IRequest<Result<FeatureFlagDto>>;
