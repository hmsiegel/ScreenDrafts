namespace ScreenDrafts.Modules.Integrations.IntegrationEvents;

public sealed class GenreModel(int tmdbId, string name)
{
  public int TmdbId { get; init; } = tmdbId;
  public string Name { get; init; } = name;
}
