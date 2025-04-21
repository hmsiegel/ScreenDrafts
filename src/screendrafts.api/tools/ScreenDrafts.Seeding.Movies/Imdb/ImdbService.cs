namespace ScreenDrafts.Seeding.Movies.Imdb;

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

  public async Task<SearchData> SearchByName(string searchExpression) =>
    await ApiLib.SearchNameAsync(searchExpression);

  public async Task<SearchData> SearchByTitle(string searchExpression) =>
    await ApiLib.SearchTitleAsync(searchExpression);

  public async Task<SearchData> SearchForMovie(string searchExpression) =>
    await ApiLib.SearchMovieAsync(searchExpression);

  public async Task<PosterData> SearchForPoster(string id) =>
    await ApiLib.PostersAsync(id);

  public async Task<SearchData> SearchForSeries(string searchExpression) =>
    await ApiLib.SearchSeriesAsync(searchExpression);

  public async Task<YouTubeTrailerData> SearchForYouTubeTrailer(string id) =>
    await ApiLib.YouTubeTrailerAsync(id);
}
