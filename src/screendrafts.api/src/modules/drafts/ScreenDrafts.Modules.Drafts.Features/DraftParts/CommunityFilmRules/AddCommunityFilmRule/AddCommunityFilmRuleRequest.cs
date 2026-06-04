namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CommunityFilmRules.AddCommunityFilmRule;

internal sealed record AddCommunityFilmRuleRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartId { get; init; } = default!;
  public int RuleKind { get; init; }
  public int? TargetSlot { get; init; }
}
