namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.GetSubDraftGameplay;

// ── Response ──────────────────────────────────────────────────────────────────

internal sealed record GetSubDraftGameplayResponse
{
  public string SubDraftPublicId { get; init; } = default!;
  public int Index { get; init; }
  public int Status { get; init; }
  public IReadOnlyList<GameplayDraftPositionResponse> DraftPositions { get; init; } = [];
  public IReadOnlyList<GameplayPickResponse> Picks { get; init; } = [];
  public IReadOnlyList<GameplayTriviaResultResponse> TriviaResults { get; init; } = [];
}
