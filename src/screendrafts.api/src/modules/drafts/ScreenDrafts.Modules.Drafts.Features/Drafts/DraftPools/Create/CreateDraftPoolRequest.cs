namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftPools.Create;

internal sealed record CreateDraftPoolRequest
{
  [FromRoute(Name = "publicId")]
  public required string PublicId { get; init; }
}
