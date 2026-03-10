namespace ScreenDrafts.Modules.Integrations.IntegrationEvents;

public sealed class DirectorModel(
  string name, string imdbId, int tmdbId)
{
  public string ImdbId { get; init; } = imdbId;
  public int TmdbId { get; init; } = tmdbId;
  public string Name { get; init; } = name;
}
