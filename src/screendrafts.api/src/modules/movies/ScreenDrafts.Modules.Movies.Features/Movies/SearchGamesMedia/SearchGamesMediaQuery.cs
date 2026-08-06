namespace ScreenDrafts.Modules.Movies.Features.Movies.SearchGamesMedia;

internal sealed record SearchGamesMediaQuery : IQuery<SearchGamesMediaResponse>
{
  public string Query { get; init; } = default!;
  public int Page { get; init; } = 1;
}
