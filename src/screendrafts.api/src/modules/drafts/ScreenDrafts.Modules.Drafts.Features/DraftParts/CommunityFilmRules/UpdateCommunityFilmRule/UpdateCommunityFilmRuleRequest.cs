namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CommunityFilmRules.UpdateCommunityFilmRule;

internal sealed record RemoveCommunityFilmRuleRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartId { get; init; } = default!;

  [FromRoute(Name = "ruleId")]
  public string RuleId { get; init; } = default!;

  public int RuleKind { get; init; }
  public int? TargetSlot { get; init; }
}
