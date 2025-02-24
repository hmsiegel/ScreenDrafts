namespace ScreenDrafts.Modules.Integrations.PublicApi;

public interface IIntegratonsApi
{
  Task<IReadOnlyCollection<MovieResponse>?> SearchForMovieByImdbId(string movieId, CancellationToken cancellationToken);
  Task<IReadOnlyCollection<MovieResponse>?> SearchForMovieByYear(string year, CancellationToken cancellationToken);
  Task<IReadOnlyCollection<MovieResponse>?> SearchForMovieByActor(string actor, CancellationToken cancellationToken);
  Task<IReadOnlyCollection<MovieResponse>?> SearchForMovieByWriter(string writer, CancellationToken cancellationToken);
  Task<IReadOnlyCollection<MovieResponse>?> SearchForMovieByDirector(string director, CancellationToken cancellationToken);
  Task<IReadOnlyCollection<MovieResponse>?> SearchForMovieByProductionComany(string productionCompany, CancellationToken cancellationToken);
  Task<MovieResponse?> GetMovieByImdbIdAsync(string ImdbId, CancellationToken cancellationToken);
}

public class MovieResponse
{
}
