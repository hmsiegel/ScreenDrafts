namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMediaByTmdbIds;

// ── Handler ───────────────────────────────────────────────────────────────────

internal sealed class GetMediaByTmdbIdsQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<GetMediaByTmdbIdsQuery, GetMediaByTmdbIdsResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<GetMediaByTmdbIdsResponse>> Handle(
    GetMediaByTmdbIdsQuery request,
    CancellationToken cancellationToken
  )
  {
    if (request.TmdbIds.Count == 0)
      return Result.Success(new GetMediaByTmdbIdsResponse());

    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql = $"""
      SELECT
        m.tmdb_id AS {nameof(MediaTmdbSummary.TmdbId)},
        m.title   AS {nameof(MediaTmdbSummary.Title)},
        m.year    AS {nameof(MediaTmdbSummary.Year)}
      FROM movies.media m
      WHERE m.tmdb_id = ANY(@TmdbIds)
        AND m.media_type = 0
      ORDER BY m.title ASC;
      """;

    var rows = await connection.QueryAsync<MediaTmdbSummary>(
      new CommandDefinition(
        sql,
        new { TmdbIds = request.TmdbIds.ToArray() },
        cancellationToken: cancellationToken
      )
    );

    return Result.Success(new GetMediaByTmdbIdsResponse { Items = [.. rows] });
  }
}
