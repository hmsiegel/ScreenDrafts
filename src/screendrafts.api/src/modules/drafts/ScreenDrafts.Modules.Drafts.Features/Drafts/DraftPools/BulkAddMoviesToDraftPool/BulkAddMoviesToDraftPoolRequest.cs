namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftPools.BulkAddMoviesToDraftPool;

internal sealed record BulkAddMoviesToDraftPoolRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;

  public required IFormFile File { get; init; }
}
