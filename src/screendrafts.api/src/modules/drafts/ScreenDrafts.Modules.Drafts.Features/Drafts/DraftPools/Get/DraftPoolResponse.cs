namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftPools.Get;

internal sealed record DraftPoolResponse
{
  public string PublicId { get; init; } = default!;
  public string DraftId { get; init; } = default!;
  public bool IsLocked { get; init; }
  public IReadOnlyList<int> TmdbIds { get; init; } = [];
}
