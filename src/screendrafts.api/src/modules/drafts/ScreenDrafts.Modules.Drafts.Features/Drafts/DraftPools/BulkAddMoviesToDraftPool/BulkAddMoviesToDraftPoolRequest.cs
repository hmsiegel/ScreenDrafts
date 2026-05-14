namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftPools.BulkAddMoviesToDraftPool;

internal sealed record BulkAddMoviesToDraftPoolRequest
{
  [FromRoute(Name = "draftId")]
  public string DraftId { get; init; } = default!;

  public required IFormFile File { get; init; }
}
