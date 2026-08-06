namespace ScreenDrafts.Modules.Integrations.Features.Games.SearchGames;

internal sealed record SearchGamesRequest
{
  [FromQuery(Name = "query")]
  public required string Query { get; init; }

  [FromQuery(Name = "page")]
  public int Page { get; init; } = 1;
}
