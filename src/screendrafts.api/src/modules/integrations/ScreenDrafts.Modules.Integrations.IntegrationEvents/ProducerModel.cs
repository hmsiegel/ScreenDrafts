namespace ScreenDrafts.Modules.Integrations.IntegrationEvents;

public sealed class ProducerModel(
  string name, string imdbId)
{
  public string ImdbId { get; init; } = imdbId;
  public string Name { get; init; } = name;
}
