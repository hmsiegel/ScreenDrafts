namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CommunityFilmRules.AssignFilmToCommunityFilmRule;

internal class AssignFilmToCommunityFilmRuleRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartId { get; set; } = default!;

  [FromRoute(Name = "ruleId")]
  public string RuleId { get; set; } = default!;
  public int TmdbId { get; set; }
}
