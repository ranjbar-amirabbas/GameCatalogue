namespace GameCatalogue.Infrastructure.Storage;

/// <summary>
/// Configuration for the MinIO object storage service.
/// </summary>
public class MinioSettings
{
    /// <summary>Gets or sets the MinIO endpoint (host:port).</summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>Gets or sets the access key.</summary>
    public string AccessKey { get; set; } = string.Empty;

    /// <summary>Gets or sets the secret key.</summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>Gets or sets the bucket name used for cover images.</summary>
    public string BucketName { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether TLS (HTTPS) is used.</summary>
    public bool UseSsl { get; set; } = false;
}
