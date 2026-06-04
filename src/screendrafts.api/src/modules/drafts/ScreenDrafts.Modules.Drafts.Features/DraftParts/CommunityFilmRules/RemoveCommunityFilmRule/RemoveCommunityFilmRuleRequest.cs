namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CommunityFilmRules.RemoveCommunityFilmRule;

internal sealed record RemoveCommunityFilmRuleRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartId { get; init; } = default!;

  [FromRoute(Name = "ruleId")]
  public string RuleId { get; init; } = default!;
}
