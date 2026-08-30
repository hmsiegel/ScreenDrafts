namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.GamePlay.GetDraftPartGamePlay;

// ── Response ──────────────────────────────────────────────────────────────────

internal sealed record GetDraftPartGameplayResponse
{
  public string DraftPartId { get; init; } = default!;
  public string DraftId { get; init; } = default!;
  public string DraftTitle { get; init; } = default!;
  public string DraftType { get; init; } = default!;
  public int PartIndex { get; init; }
  public bool IsMultiPart { get; init; }
  public bool IsFinalPart { get; init; }
  public bool HasDraftPool { get; init; }
  public bool HasDraftBoard { get; init; }
  public bool HasCandidateList { get; init; }
  public CurrentUserRolesResponse CurrentUserRoles { get; init; } = new();
  public string? CallerParticipantId { get; init; }

  public string? FungibleTokenName { get; init; }

  public int? RestrictedTvSeriesTmdbId { get; init; }

  /// <summary>
  /// True when this draft has no dedicated host — see Draft.IsHostless's remarks. When
  /// true, reveal authority for a pick belongs to whichever participant it's sent to
  /// (see GameplayPickResponse.RevealAuthorizedParticipantId) rather than the primary host.
  /// </summary>
  public bool IsHostless { get; init; }

  /// <summary>
  /// Legends Mega's Booster's Champion pick reservations for this part — see
  /// BoostersChampionAssignment's remarks. Deliberately separate from CommunityFilmRules;
  /// the two are unrelated mechanisms.
  /// </summary>
  public IReadOnlyList<GameplayBoostersChampionAssignmentResponse> BoostersChampionAssignments { get; init; } =
  [];
  public IReadOnlyList<GameplayTriviaResultResponse> TriviaResults { get; init; } = [];
  public IReadOnlyList<GameplayDraftPositionResponse> DraftPositions { get; init; } = [];
  public string? NextExpectedParticipantId { get; init; }
  public int? NextExpectedParticipantKind { get; init; }
  public IReadOnlyList<GameplayParticipantResponse> Participants { get; init; } = [];
  public IReadOnlyList<GameplayPickResponse> Picks { get; init; } = [];
  public IReadOnlyList<GameplayHostResponse> Hosts { get; init; } = [];
  public IReadOnlyList<GameplayCommunityFilmRuleResponse> CommunityFilmRules { get; init; } = [];
  public IReadOnlyList<GameplaySubDraftSummaryResponse> SubDrafts { get; init; } = [];
}
