namespace ScreenDrafts.Modules.Integrations.IntegrationEvents;

public sealed class DirectorModel(
  string name, string imdbId)
{
  public string ImdbId { get; init; } = imdbId;
  public string Name { get; init; } = name;
}
