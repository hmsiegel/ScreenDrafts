namespace ScreenDrafts.Modules.Movies.PublicApi;

public interface IMovieTitleReader
{
  Task<IReadOnlyDictionary<string, string>> GetTitlesByPublicIdsAsync(
    IEnumerable<string> publicIds,
    CancellationToken cancellationToken = default
  );

  Task<IReadOnlyList<string>> GetPublicIdsByTmdbIdsAsync(
    IReadOnlyList<int> tmdbIds,
    CancellationToken cancellationToken = default
  );
}
