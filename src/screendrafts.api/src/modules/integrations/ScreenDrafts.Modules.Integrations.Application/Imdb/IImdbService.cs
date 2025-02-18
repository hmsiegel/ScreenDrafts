namespace ScreenDrafts.Modules.Integrations.Application.Imdb;

public interface IImdbService
{
  Task<AdvancedSearchData> AdvancedSearch(AdvancedSearchInput advancedSearchQuery);
  Task<FullCastData> GetFullCast(string id);
  Task<TitleData> GetMovieInformation(string id, TitleOptions? options);
  Task<TitleData> GetMovieInformation(string id);
  Task<SearchData> SearchByKeyword(string searchExpression);
  Task<SearchData> SearchByTitle(string searchExpression);
  Task<SearchData> SearchForMovie(string searchExpression);
  Task<SearchData> SearchForSeries(string searchExpression);
  Task<YouTubeTrailerData> SearchForYouTubeTrailer(string id);
  Task<SearchData> SearchByName(string searchExpression);
  Task<PosterData> SearchForPoster(string id);
}
