using GameCatalogue.Application.Interfaces.Storage;
using MediatR;

namespace GameCatalogue.Application.Queries.GetGameImage;

/// <summary>
/// Query to load a cover image by its storage key, optionally resized to a target
/// width (used for fast-loading list thumbnails).
/// </summary>
public record GetGameImageQuery(string FileKey, int? Width = null) : IRequest<StoredFile?>;
