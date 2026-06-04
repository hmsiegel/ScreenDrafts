namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CommunityFilmRules.RemoveCommunityFilmRule;

internal sealed record RemoveCommunityFilmRuleCommand : ICommand
{
  public required string DraftPartId { get; init; }
  public required string RuleId { get; init; }
}
