namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CommunityFilmRules.AddCommunityFilmRule;

internal sealed record AddCommunityFilmRuleCommand : ICommand<string>
{
  public required string DraftPartId { get; init; }
  public int RuleKind { get; init; } = default!;
  public int? TargetSlot { get; init; }
}
