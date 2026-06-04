namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Get;

internal sealed record GetDraftCommunityFilmRuleResponse
{
  public string PublicId { get; init; } = default!;
  public CommunityFilmRuleKind RuleKind { get; init; } = default!;
  public int? TargetSlot { get; init; }
  public int? TmdbId { get; init; }
  public string? Title { get; init; }
}
