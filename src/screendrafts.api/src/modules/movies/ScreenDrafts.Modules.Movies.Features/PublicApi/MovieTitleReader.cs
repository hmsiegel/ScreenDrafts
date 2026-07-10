namespace ScreenDrafts.Modules.Movies.Features.PublicApi;

internal sealed class MovieTitleReader(IDbConnectionFactory connectionFactory) : IMovieTitleReader
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

  public async Task<IReadOnlyList<string>> GetPublicIdsByTmdbIdsAsync(
    IReadOnlyList<int> tmdbIds,
    CancellationToken cancellationToken = default
  )
  {
    if (tmdbIds.Count == 0)
    {
      return [];
    }

    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql = """
      SELECT m.public_id
      FROM movies.media m
      WHERE m.tmdb_id = ANY(@TmdbIds)
      """;

    var publicIds = await connection.QueryAsync<string>(
      new CommandDefinition(
        commandText: sql,
        parameters: new { TmdbIds = tmdbIds },
        cancellationToken: cancellationToken
      )
    );

    return [.. publicIds];
  }

  public async Task<IReadOnlyDictionary<string, string>> GetTitlesByPublicIdsAsync(
    IEnumerable<string> publicIds,
    CancellationToken cancellationToken = default
  )
  {
    var ids = publicIds as string[] ?? [.. publicIds];

    if (ids.Length == 0)
    {
      return new Dictionary<string, string>();
    }

    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql = """
      SELECT 
        public_id as PublicId, 
        title as Title
      FROM movies.media
      WHERE public_id = ANY(@PublicIds)
      """;

    var rows = await connection.QueryAsync<MovieTitleRow>(
      new CommandDefinition(
        commandText: sql,
        parameters: new { PublicIds = ids },
        cancellationToken: cancellationToken
      )
    );

    return rows.ToDictionary(r => r.PublicId, r => r.Title);
  }

  private sealed record MovieTitleRow(string PublicId, string Title);
}
