namespace GameCatalogue.Application.Interfaces.Storage;

/// <summary>
/// Abstraction over an object storage service for binary assets.
/// </summary>
public interface IStorageService
{
    /// <summary>Uploads a binary object and returns its storage key (file key).</summary>
    /// <param name="stream">The content stream.</param>
    /// <param name="fileName">The original file name.</param>
    /// <param name="contentType">The MIME content type.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The storage object key of the uploaded file.</returns>
    Task<string> UploadAsync(Stream stream, string fileName, string contentType, CancellationToken ct);

    /// <summary>Downloads a stored object by its key.</summary>
    /// <param name="fileKey">The storage object key.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The file content and content type, or <c>null</c> if not found.</returns>
    Task<StoredFile?> DownloadAsync(string fileKey, CancellationToken ct);

    /// <summary>Deletes a stored object by its key.</summary>
    /// <param name="fileKey">The storage object key to delete.</param>
    /// <param name="ct">A cancellation token.</param>
    Task DeleteAsync(string fileKey, CancellationToken ct);
}
