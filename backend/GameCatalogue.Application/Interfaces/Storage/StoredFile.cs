namespace GameCatalogue.Application.Interfaces.Storage;

/// <summary>
/// The content and metadata of a file retrieved from object storage.
/// </summary>
/// <param name="Content">The raw file bytes.</param>
/// <param name="ContentType">The MIME content type.</param>
public record StoredFile(byte[] Content, string ContentType);
