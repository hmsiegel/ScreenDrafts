namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.GamePlay.GetDraftPartGamePlay;

// ── Response ──────────────────────────────────────────────────────────────────

internal sealed record GetDraftPartGameplayResponse
{
  public string DraftPartId { get; init; } = default!;
  public string DraftId { get; init; } = default!;
  public string DraftTitle { get; init; } = default!;
  public string DraftType { get; init; } = default!;
  public bool IsMultiPart { get; init; }
  public bool IsFinalPart { get; init; }
  public bool HasDraftPool { get; init; }
  public bool HasDraftBoard { get; init; }
  public bool HasCandidateList { get; init; }
  public IReadOnlyList<GameplayTriviaResultResponse> TriviaResults { get; init; } = [];
  public IReadOnlyList<GameplayDraftPositionResponse> DraftPositions { get; init; } = [];
  public string? NextExpectedParticipantId { get; init; }
  public int? NextExpectedParticipantKind { get; init; }
  public IReadOnlyList<GameplayParticipantResponse> Participants { get; init; } = [];
  public IReadOnlyList<GameplayPickResponse> Picks { get; init; } = [];
  public IReadOnlyList<GameplayHostResponse> Hosts { get; init; } = [];
}
