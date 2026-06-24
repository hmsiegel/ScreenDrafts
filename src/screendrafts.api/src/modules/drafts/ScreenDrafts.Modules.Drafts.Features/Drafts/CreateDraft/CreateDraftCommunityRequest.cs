namespace ScreenDrafts.Modules.Drafts.Features.Drafts.CreateDraft;

internal sealed record CreateDraftCommunityRequest
{
  public required int MaxCommunityPicks { get; init; }
  public required int MaxCommunityVetoes { get; init; }
  public IReadOnlyList<CreateDraftCommunityFilmRuleRequest> FilmRules { get; init; } = [];
}
