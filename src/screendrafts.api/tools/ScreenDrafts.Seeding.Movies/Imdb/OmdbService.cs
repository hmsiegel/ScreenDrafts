namespace ScreenDrafts.Seeding.Movies.Imdb;

internal sealed class OmdbService(IOptions<OmdbSettings> omdbSettings) : IOmdbService
{
  private readonly OmdbSettings _omdbSettings = omdbSettings.Value;

  private AsyncOmdbClient OmdbClient => new(_omdbSettings.Key);

  public async Task<Item> GetItemByTitleAsync(string title, bool fullPlot) =>
    await OmdbClient.GetItemByTitleAsync(title, fullPlot);

  public async Task<Item> GetItemByIdAsync(string id, bool fullPlot) =>
    await OmdbClient.GetItemByIdAsync(id, fullPlot);

  public async Task<Item> GetSeriesByTitleAsync(string id, bool fullPlot) =>
    await OmdbClient.GetItemByTitleAsync(id, OmdbType.Series, fullPlot);
}

