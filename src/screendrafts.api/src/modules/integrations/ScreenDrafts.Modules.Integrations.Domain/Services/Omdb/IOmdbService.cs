using OMDbApiNet.Model;

namespace ScreenDrafts.Modules.Integrations.Domain.Services.Omdb;

public interface IOmdbService
{
  Task<Item> GetItemByIdAsync(string id, bool fullPlot);
  Task<Item> GetItemByTitleAsync(string title, bool fullPlot);
  Task<Item> GetSeriesByTitleAsync(string id, bool fullPlot);
  Task<SearchList> SearchAsync(string query, int page);
}
