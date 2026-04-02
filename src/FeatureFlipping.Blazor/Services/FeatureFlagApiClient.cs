using System.Net.Http.Json;

namespace FeatureFlipping.Blazor.Services;

/// <summary>HTTP client implementation for the feature flag API.</summary>
public sealed class FeatureFlagApiClient : IFeatureFlagApiClient
{
    private readonly HttpClient _http;

    /// <summary>Initializes the client.</summary>
    public FeatureFlagApiClient(HttpClient http) => _http = http;

    /// <inheritdoc/>
    public async Task<List<FeatureFlagModel>> GetAllFlagsAsync() =>
        await _http.GetFromJsonAsync<List<FeatureFlagModel>>("/api/flags") ?? [];

    /// <inheritdoc/>
    public async Task<FeatureFlagModel?> GetFlagAsync(string key) =>
        await _http.GetFromJsonAsync<FeatureFlagModel>($"/api/flags/{key}");

    /// <inheritdoc/>
    public async Task<FeatureFlagModel?> ToggleFlagAsync(string key)
    {
        var response = await _http.PatchAsync($"/api/flags/{key}/toggle", null);
        return await response.Content.ReadFromJsonAsync<FeatureFlagModel>();
    }

    /// <inheritdoc/>
    public async Task<FeatureFlagModel?> UpdateFlagAsync(string key, UpdateFlagRequest request)
    {
        var response = await _http.PutAsJsonAsync($"/api/flags/{key}", request);
        return await response.Content.ReadFromJsonAsync<FeatureFlagModel>();
    }

    /// <inheritdoc/>
    public async Task<FlagEvaluationModel?> EvaluateFlagAsync(string key, string? userId)
    {
        var url = string.IsNullOrEmpty(userId) ? $"/api/flags/{key}/evaluate" : $"/api/flags/{key}/evaluate?userId={Uri.EscapeDataString(userId)}";
        return await _http.GetFromJsonAsync<FlagEvaluationModel>(url);
    }

    /// <inheritdoc/>
    public async Task<string> GetCacheStatusAsync(string key)
    {
        var result = await _http.GetFromJsonAsync<CacheStatusResponse>($"/api/flags/{key}/cache-status");
        return result?.Status ?? "FRESH";
    }

    private sealed record CacheStatusResponse(string Status);
}
