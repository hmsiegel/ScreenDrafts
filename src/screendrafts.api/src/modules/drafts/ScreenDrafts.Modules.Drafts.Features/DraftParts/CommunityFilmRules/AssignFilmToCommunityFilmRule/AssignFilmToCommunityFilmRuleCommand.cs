namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CommunityFilmRules.AssignFilmToCommunityFilmRule;

internal class AssignFilmToCommunityFilmRuleCommand : ICommand
{
  public required string DraftPartId { get; set; }
  public required string RuleId { get; set; }
  public int TmdbId { get; set; }
}
