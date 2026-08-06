namespace ScreenDrafts.Modules.Integrations.Features.Games.SearchGames;

internal sealed record SearchGamesResponse
{
  public IReadOnlyList<GameSearchResult> Results { get; init; } = [];
}
