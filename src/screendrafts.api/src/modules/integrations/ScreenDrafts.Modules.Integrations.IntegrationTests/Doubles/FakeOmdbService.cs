using OMDbApiNet.Model;

namespace ScreenDrafts.Modules.Integrations.IntegrationTests.Doubles;

public sealed class FakeOmdbService : IOmdbService
{
  private SearchList _searchList = new() { SearchResults = [] };
  private Item? _item;

  public void SetSearchList(SearchList searchList) => _searchList = searchList;

  public void SetItem(Item? item) => _item = item;

  public void Reset()
  {
    _searchList = new SearchList { SearchResults = [] };
    _item = null;
  }

  public Task<Item> GetItemByIdAsync(string id, bool fullPlot) => Task.FromResult(_item!);

  public Task<Item> GetItemByTitleAsync(string title, bool fullPlot) => Task.FromResult(_item!);

  public Task<Item> GetSeriesByTitleAsync(string id, bool fullPlot) => Task.FromResult(_item!);

  public Task<SearchList> SearchAsync(string query, int page) => Task.FromResult(_searchList);
}
