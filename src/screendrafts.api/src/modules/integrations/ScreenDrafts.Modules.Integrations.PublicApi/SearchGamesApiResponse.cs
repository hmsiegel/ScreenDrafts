namespace ScreenDrafts.Modules.Integrations.PublicApi;

public sealed record SearchGamesApiResponse
{
  public IReadOnlyList<GameSearchApiResult> Results { get; init; } = [];
}
