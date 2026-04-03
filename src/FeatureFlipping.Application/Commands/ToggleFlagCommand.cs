using FeatureFlipping.Application.Common;
using FeatureFlipping.Application.DTOs;
using MediatR;

namespace FeatureFlipping.Application.Commands;

/// <summary>Command to toggle a feature flag's IsEnabled state.</summary>
public sealed record ToggleFlagCommand(string Key) : IRequest<Result<FeatureFlagDto>>;
