namespace ScreenDrafts.Modules.Drafts.Features.Drafts.CreateDraft;

internal sealed record CreateDraftCommunityFilmRuleRequest
{
  public required int RuleKind { get; init; }
  public int? TargetSlot { get; init; }
  public int? TmdbId { get; init; }
}
