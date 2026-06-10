using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GameCatalogue.API;

/// <summary>
/// Writes a detailed JSON health report (overall status plus each dependency)
/// so the health dashboard page can render component-level status.
/// </summary>
public static class HealthResponseWriter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>Serializes a <see cref="HealthReport"/> to the response as JSON.</summary>
    public static Task WriteJsonAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var payload = new
        {
            status = report.Status.ToString(),
            totalDurationMs = Math.Round(report.TotalDuration.TotalMilliseconds, 1),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                durationMs = Math.Round(entry.Value.Duration.TotalMilliseconds, 1),
                error = entry.Value.Exception?.Message
            })
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(payload, Options));
    }
}
