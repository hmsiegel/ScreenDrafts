namespace ScreenDrafts.Modules.Integrations.IntegrationEvents;

public sealed class GenreModel(int tmdb, string name)
{
  public int Tmdb { get; init; } = tmdb;
  public string Name { get; init; } = name;
}
