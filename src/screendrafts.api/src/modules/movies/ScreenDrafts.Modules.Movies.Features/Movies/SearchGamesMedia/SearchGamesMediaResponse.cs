namespace ScreenDrafts.Modules.Movies.Features.Movies.SearchGamesMedia;

internal sealed record SearchGamesMediaResponse
{
  public IReadOnlyList<GameMediaSearchResultResponse> Results { get; init; } = [];
}
