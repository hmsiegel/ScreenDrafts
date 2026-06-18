namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.GamePlay.GetDraftPartGamePlay;

// ── Query ─────────────────────────────────────────────────────────────────────

internal sealed record GetDraftPartGameplayQuery : IQuery<GetDraftPartGameplayResponse>
{
  public required string DraftPartPublicId { get; init; }
  public required Guid? CallerUserId { get; init; }
}
