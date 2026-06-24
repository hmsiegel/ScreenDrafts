namespace ScreenDrafts.Modules.Drafts.Features.Drafts.CreateDraft;

internal sealed record CommunityInput
{
  public required int MaxCommunityPicks { get; init; }
  public required int MaxCommunityVetoes { get; init; }
  public IReadOnlyList<CommunityFilmRuleInput> FilmRules { get; init; } = [];
}
