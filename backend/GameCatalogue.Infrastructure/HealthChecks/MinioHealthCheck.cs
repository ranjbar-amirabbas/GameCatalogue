using Microsoft.Extensions.Diagnostics.HealthChecks;
using Minio;

namespace GameCatalogue.Infrastructure.HealthChecks;

/// <summary>
/// Health check that verifies MinIO connectivity by listing buckets.
/// </summary>
public class MinioHealthCheck : IHealthCheck
{
    private readonly IMinioClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="MinioHealthCheck"/> class.
    /// </summary>
    /// <param name="client">The MinIO client.</param>
    public MinioHealthCheck(IMinioClient client)
    {
        _client = client;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _client.ListBucketsAsync(cancellationToken);
            return HealthCheckResult.Healthy("MinIO is reachable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("MinIO is not reachable.", ex);
        }
    }
}
