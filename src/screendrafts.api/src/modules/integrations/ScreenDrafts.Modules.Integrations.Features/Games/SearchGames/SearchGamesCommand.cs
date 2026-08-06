namespace ScreenDrafts.Modules.Integrations.Features.Games.SearchGames;

internal sealed record SearchGamesCommand : ICommand<SearchGamesResponse>
{
  public required string Query { get; init; }
  public int Page { get; init; } = 1;
}
