namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftPools.Get;

internal sealed record GetDraftPoolQuery : IQuery<DraftPoolResponse>
{
  public required string PublicId { get; init; }
}
