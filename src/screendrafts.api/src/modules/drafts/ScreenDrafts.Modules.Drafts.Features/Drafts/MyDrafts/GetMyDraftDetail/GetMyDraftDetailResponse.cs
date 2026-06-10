namespace ScreenDrafts.Modules.Drafts.Features.Drafts.MyDrafts.GetMyDraftDetail;

// ── Response ──────────────────────────────────────────────────────────────────

internal sealed record GetMyDraftDetailResponse
{
  public string DraftPublicId { get; init; } = default!;
  public string Title { get; init; } = default!;
  public int DraftType { get; init; }
  public bool HasPool { get; init; }
  public bool IsSurrogate { get; init; }

  /// <summary>Distinct roles the caller holds on this draft across all parts.</summary>
  public IReadOnlyList<string> MyRoles { get; init; } = [];

  public IReadOnlyList<MyDraftPartDetail> Parts { get; init; } = [];
}
