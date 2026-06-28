namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.GamePlay.GetDraftPartGamePlay;

internal sealed record GameplayCommunityFilmRuleResponse
{
  public string PublicId { get; init; } = default!;
  public int RuleKind { get; init; }
  public int? TargetSlot { get; init; }
  public int? TmdbId { get; init; }
  public string? Title { get; init; }
  public bool WasAutoVetoFired { get; init; }
}
