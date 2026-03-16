namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftPools.BulkAddMoviesToDraftPool;

internal sealed record BulkAddMoviesToDraftPoolRequest
{
  [FromRoute(Name = "draftId")]
  public required string DraftId { get; init; }

  public required IFormFile File { get; init; }
}
