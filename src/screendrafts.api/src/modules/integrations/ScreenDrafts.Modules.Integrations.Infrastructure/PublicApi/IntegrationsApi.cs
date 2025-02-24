namespace ScreenDrafts.Modules.Integrations.Infrastructure.PublicApi;

internal sealed class IntegrationsApi : IIntegratonsApi
{
  public Task<MovieResponse?> GetMovieByImdbIdAsync(string ImdbId, CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }

  public Task<IReadOnlyCollection<MovieResponse>?> SearchForMovieByActor(string actor, CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }

  public Task<IReadOnlyCollection<MovieResponse>?> SearchForMovieByDirector(string director, CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }

  public Task<IReadOnlyCollection<MovieResponse>?> SearchForMovieByImdbId(string movieId, CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }

  public Task<IReadOnlyCollection<MovieResponse>?> SearchForMovieByProductionComany(string productionCompany, CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }

  public Task<IReadOnlyCollection<MovieResponse>?> SearchForMovieByWriter(string writer, CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }

  public Task<IReadOnlyCollection<MovieResponse>?> SearchForMovieByYear(string year, CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }
}
