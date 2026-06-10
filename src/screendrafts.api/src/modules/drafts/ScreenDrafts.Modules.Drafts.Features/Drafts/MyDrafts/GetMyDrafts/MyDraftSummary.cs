namespace ScreenDrafts.Modules.Drafts.Features.Drafts.MyDrafts.GetMyDrafts;

internal sealed record MyDraftSummary
{
  public string DraftPublicId { get; init; } = default!;
  public string Title { get; init; } = default!;
  public int DraftType { get; init; }
  public bool HasPool { get; init; }
  public int DraftStatus { get; init; }
  public IReadOnlyList<MyDraftPartSummary> Parts { get; init; } = [];
}
