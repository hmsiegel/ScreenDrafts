namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMediaByImdbIds;

internal sealed class GetMediaByImdbIdsQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<GetMediaByImdbIdsQuery, GetMediaByImdbIdsResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<GetMediaByImdbIdsResponse>> Handle(
    GetMediaByImdbIdsQuery request,
    CancellationToken cancellationToken
  )
  {
    if (request.ImdbIds.Count == 0)
      return Result.Success(new GetMediaByImdbIdsResponse());

    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql = $"""
      SELECT
        m.public_id AS {nameof(MediaImdbSummary.PublicId)},
        m.imdb_id AS {nameof(MediaImdbSummary.ImdbId)},
        m.title   AS {nameof(MediaImdbSummary.Title)},
        m.year    AS {nameof(MediaImdbSummary.Year)},
        m.image AS {nameof(MediaImdbSummary.PosterUrl)}
      FROM movies.media m
      WHERE m.imdb_id = ANY(@ImdbIds)
      ORDER BY m.title ASC;
      """;

    var rows = await connection.QueryAsync<MediaImdbSummary>(
      new CommandDefinition(
        sql,
        new { ImdbIds = request.ImdbIds.ToArray() },
        cancellationToken: cancellationToken
      )
    );

    return Result.Success(new GetMediaByImdbIdsResponse { Items = [.. rows] });
  }
}
