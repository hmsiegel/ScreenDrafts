namespace ScreenDrafts.Modules.Integrations.Application.Imdb;
public interface IOmdbService
{
  Task<Item> GetItemByIdAsync(string id, bool fullPlot);
  Task<Item> GetItemByTitleAsync(string title, bool fullPlot);
  Task<Item> GetSeriesByTitleAsync(string id, bool fullPlot);
}
