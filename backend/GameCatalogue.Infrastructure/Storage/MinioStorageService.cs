using GameCatalogue.Application.Interfaces.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace GameCatalogue.Infrastructure.Storage;

/// <summary>
/// MinIO-backed implementation of <see cref="IStorageService"/>.
/// </summary>
public class MinioStorageService : IStorageService
{
    private readonly IMinioClient _client;
    private readonly MinioSettings _settings;
    private readonly ILogger<MinioStorageService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MinioStorageService"/> class.
    /// </summary>
    public MinioStorageService(
        IMinioClient client,
        IOptions<MinioSettings> settings,
        ILogger<MinioStorageService> logger)
    {
        _client = client;
        _settings = settings.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<string> UploadAsync(Stream stream, string fileName, string contentType, CancellationToken ct)
    {
        var bucket = _settings.BucketName;

        var exists = await _client.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(bucket), ct);

        if (!exists)
        {
            await _client.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucket), ct);
            _logger.LogInformation("Created storage bucket {Bucket}", bucket);
        }

        var objectName = $"{Guid.NewGuid()}_{fileName}";

        await _client.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(bucket)
                .WithObject(objectName)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType(contentType),
            ct);

        _logger.LogInformation("Uploaded object {Object} to bucket {Bucket}", objectName, bucket);
        return objectName;
    }

    /// <inheritdoc />
    public async Task<StoredFile?> DownloadAsync(string fileKey, CancellationToken ct)
    {
        var bucket = _settings.BucketName;

        try
        {
            var stat = await _client.StatObjectAsync(
                new StatObjectArgs().WithBucket(bucket).WithObject(fileKey), ct);

            using var memory = new MemoryStream();
            await _client.GetObjectAsync(
                new GetObjectArgs()
                    .WithBucket(bucket)
                    .WithObject(fileKey)
                    .WithCallbackStream(async (source, token) => await source.CopyToAsync(memory, token)),
                ct);

            var contentType = string.IsNullOrWhiteSpace(stat.ContentType)
                ? "application/octet-stream"
                : stat.ContentType;

            return new StoredFile(memory.ToArray(), contentType);
        }
        catch (Minio.Exceptions.ObjectNotFoundException)
        {
            return null;
        }
        catch (Minio.Exceptions.BucketNotFoundException)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string fileKey, CancellationToken ct)
    {
        await _client.RemoveObjectAsync(
            new RemoveObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(fileKey),
            ct);

        _logger.LogInformation("Deleted object {Object} from bucket {Bucket}", fileKey, _settings.BucketName);
    }
}
