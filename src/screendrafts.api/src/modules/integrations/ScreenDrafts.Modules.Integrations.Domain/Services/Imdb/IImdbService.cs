namespace ScreenDrafts.Modules.Integrations.Domain.Services.Imdb;

public interface IImdbService
{
  Task<AdvancedSearchData?> AdvancedSearch(AdvancedSearchInput advancedSearchQuery);
  Task<FullCastData?> GetFullCast(string id);
  Task<TitleData?> GetMovieInformation(string id, TitleOptions? options);
  Task<TitleData?> GetMovieInformation(string id);
  Task<SearchKeywordData?> SearchByKeyword(string searchExpression);
  Task<SearchTitleData?> SearchByTitle(string searchExpression);
  Task<SearchTitleData?> SearchForMovie(string searchExpression);
  Task<SearchTitleData?> SearchForSeries(string searchExpression);
  Task<YouTubeTrailerData?> SearchForYouTubeTrailer(string id);
  Task<SearchNameData?> SearchByName(string searchExpression);
  Task<PosterData?> SearchForPoster(string id);
  Task<NameData?> GetPersonInformation(string nmId);
}
