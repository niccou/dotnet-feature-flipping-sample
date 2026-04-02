using FeatureFlipping.Application.Common;
using FeatureFlipping.Application.DTOs;
using MediatR;

namespace FeatureFlipping.Application.Commands;

/// <summary>Command to update a feature flag.</summary>
public sealed record UpdateFlagCommand(string Key, UpdateFeatureFlagDto Dto) : IRequest<Result<FeatureFlagDto>>;
