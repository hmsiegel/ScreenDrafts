using IMDbApiLib;
using IMDbApiLib.Models;
using ScreenDrafts.Modules.Integrations.Application.Imdb;
using ScreenDrafts.Modules.Integrations.Domain.Enums;

namespace ScreenDrafts.Modules.Integrations.Infrastructure.Imdb;

internal sealed class ImdbService(IOptions<ImdbSettings> imdbSettings) : IImdbService
{
  private readonly ImdbSettings _imdbSettings = imdbSettings.Value;

  private ApiLib ApiLib => new(_imdbSettings.Key);

  public async Task<AdvancedSearchData> AdvancedSearch(AdvancedSearchInput advancedSearchQuery) =>
    await ApiLib.AdvancedSearchAsync(advancedSearchQuery);

  public Task<FullCastData> GetFullCast(string id) =>
    ApiLib.FullCastDataAsync(id);

  public async Task<TitleData> GetMovieInformation(string id, TitleOptions? options) =>
    await ApiLib.TitleAsync(
      id,
      Language.en,
      options.ToString());

  public async Task<TitleData> GetMovieInformation(string id) =>
    await ApiLib.TitleAsync(id, Language.en);

  public async Task<SearchData> SearchByKeyword(string searchExpression) =>
    await ApiLib.SearchKeywordAsync(searchExpression);

  public async Task<SearchData> SearchByTitle(string searchExpression) =>
    await ApiLib.SearchTitleAsync(searchExpression);

  public async Task<SearchData> SearchForMovie(string searchExpression) =>
    await ApiLib.SearchMovieAsync(searchExpression);

  public async Task<SearchData> SearchForSeries(string searchExpression) =>
    await ApiLib.SearchSeriesAsync(searchExpression);
}
