namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CommunityFilmRules.AssignFilmToCommunityFilmRule;

internal sealed record AssignFilmToCommunityFilmRuleCommand : ICommand
{
  public required string DraftPartId { get; set; }
  public required string RuleId { get; set; }
  public int TmdbId { get; set; }
}
