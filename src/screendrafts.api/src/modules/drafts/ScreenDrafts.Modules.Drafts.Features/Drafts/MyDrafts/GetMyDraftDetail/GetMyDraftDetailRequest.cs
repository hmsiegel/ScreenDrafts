namespace ScreenDrafts.Modules.Drafts.Features.Drafts.MyDrafts.GetMyDraftDetail;

// ── Request ───────────────────────────────────────────────────────────────────

internal sealed record GetMyDraftDetailRequest
{
  [FromRoute(Name = "draftId")]
  public string DraftId { get; init; } = default!;
}
