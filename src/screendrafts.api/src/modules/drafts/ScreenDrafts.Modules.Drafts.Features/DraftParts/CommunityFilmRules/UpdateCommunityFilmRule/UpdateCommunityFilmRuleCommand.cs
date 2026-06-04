namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CommunityFilmRules.UpdateCommunityFilmRule;

internal sealed record RemoveCommunityFilmRuleCommand : ICommand
{
  public required string DraftPartId { get; init; }
  public required string RuleId { get; init; }
  public int RuleKind { get; init; } = default!;
  public int? TargetSlot { get; init; }
}
