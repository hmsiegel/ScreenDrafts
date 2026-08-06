namespace ScreenDrafts.Modules.Integrations.Features.Games.SearchGames;

internal sealed record GameSearchResult
{
  public int IgdbId { get; init; }
  public string Title { get; init; } = string.Empty;
  public string? Year { get; init; }
  public string? PosterUrl { get; init; }
}
