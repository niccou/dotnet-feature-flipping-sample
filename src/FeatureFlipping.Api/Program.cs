using FeatureFlipping.Application.Commands;
using FeatureFlipping.Application.Queries;
using FeatureFlipping.Application.Abstractions;
using FeatureFlipping.Domain.Aggregates;
using FeatureFlipping.Domain.ValueObjects;
using FeatureFlipping.Infrastructure;
using FeatureFlipping.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(FeatureFlipping.Application.Handlers.GetAllFlagsQueryHandler).Assembly));
builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=featureflags.db");
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FeatureFlagDbContext>();
    db.Database.Migrate();

    var repo = scope.ServiceProvider.GetRequiredService<FeatureFlipping.Domain.Interfaces.IFeatureFlagRepository>();
    if (!await repo.ExistsAsync(new FlagKey("dark-mode")))
    {
        await repo.AddAsync(FeatureFlag.Create(new FlagKey("dark-mode"), isEnabled: true, rolloutPercentage: 100));
        await repo.AddAsync(FeatureFlag.Create(new FlagKey("new-checkout-flow"), isEnabled: false, userTargeting: ["user-42"], rolloutPercentage: 0));
        await repo.AddAsync(FeatureFlag.Create(new FlagKey("ai-recommendations"), isEnabled: true, rolloutPercentage: 30));
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.MapGet("/api/flags", async (IMediator mediator) =>
{
    var result = await mediator.Send(new GetAllFlagsQuery());
    return result.IsSuccess ? Results.Ok(result.Value) : Results.Problem(result.Error);
});

app.MapGet("/api/flags/{key}", async (string key, IMediator mediator) =>
{
    var result = await mediator.Send(new GetFlagByKeyQuery(key));
    return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound(result.Error);
});

app.MapPost("/api/flags", async (FeatureFlipping.Application.DTOs.CreateFeatureFlagDto dto, IMediator mediator) =>
{
    var result = await mediator.Send(new CreateFlagCommand(dto));
    return result.IsSuccess ? Results.Created($"/api/flags/{result.Value!.Key}", result.Value) : Results.BadRequest(result.Error);
});

app.MapPut("/api/flags/{key}", async (string key, FeatureFlipping.Application.DTOs.UpdateFeatureFlagDto dto, IMediator mediator) =>
{
    var result = await mediator.Send(new UpdateFlagCommand(key, dto));
    return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound(result.Error);
});

app.MapMethods("/api/flags/{key}/toggle", ["PATCH"], async (string key, IMediator mediator) =>
{
    var result = await mediator.Send(new ToggleFlagCommand(key));
    return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound(result.Error);
});

app.MapGet("/api/flags/{key}/evaluate", async (string key, string? userId, IMediator mediator) =>
{
    var result = await mediator.Send(new EvaluateFlagQuery(key, userId));
    return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound(result.Error);
});

app.MapGet("/api/flags/{key}/evaluate/live", async (string key, string? userId, IMediator mediator, HttpContext httpContext) =>
{
    httpContext.Response.Headers.Append("Content-Type", "text/event-stream");
    httpContext.Response.Headers.Append("Cache-Control", "no-cache");
    httpContext.Response.Headers.Append("X-Accel-Buffering", "no");

    var ct = httpContext.RequestAborted;
    while (!ct.IsCancellationRequested)
    {
        var result = await mediator.Send(new EvaluateFlagQuery(key, userId), ct);
        if (result.IsSuccess)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(result.Value);
            await httpContext.Response.WriteAsync($"data: {json}\n\n", ct);
            await httpContext.Response.Body.FlushAsync(ct);
        }
        await Task.Delay(2000, ct);
    }
});

app.MapGet("/api/flags/{key}/cache-status", async (string key, IFeatureFlagCache cache) =>
{
    FlagKey flagKey;
    try { flagKey = new FlagKey(key); }
    catch { return Results.BadRequest("Invalid flag key"); }

    var isCached = await cache.IsCachedAsync(flagKey);
    return Results.Ok(new { status = isCached ? "CACHED" : "FRESH" });
});

app.Run();
