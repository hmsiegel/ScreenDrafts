namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetDraftPartStatus;

internal sealed record Response
{
  public string DraftPublicId { get; init; } = default!;
  public int PartIndex { get; init; }
  public Guid DraftPartId { get; init; }
  public string DraftStatus { get; init; } = default!;
  public string DraftLifecylce { get; init; } = default!;
  public string DraftPartStatus { get; init; } = default!;
  public string DraftPartLifecycle { get; init; } = default!;
}
