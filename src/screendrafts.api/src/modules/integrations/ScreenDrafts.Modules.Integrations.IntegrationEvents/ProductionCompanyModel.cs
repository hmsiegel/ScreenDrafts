namespace ScreenDrafts.Modules.Integrations.IntegrationEvents;

public class ProductionCompanyModel(string imdbId, string name)
{
  public string ImdbId { get; init; } = imdbId;
  public string Name { get; init; } = name;
}
