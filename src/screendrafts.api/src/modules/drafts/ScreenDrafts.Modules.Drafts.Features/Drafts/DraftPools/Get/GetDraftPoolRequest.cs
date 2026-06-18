namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftPools.Get;

internal sealed record GetDraftPoolRequest
{
  [FromRoute(Name = "publicId")]
  public required string PublicId { get; init; }
}
