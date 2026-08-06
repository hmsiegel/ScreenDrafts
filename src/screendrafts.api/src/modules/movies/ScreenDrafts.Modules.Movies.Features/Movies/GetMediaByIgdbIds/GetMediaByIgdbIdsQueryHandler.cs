namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMediaByIgdbIds;

internal sealed class GetMediaByIgdbIdsQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<GetMediaByIgdbIdsQuery, GetMediaByIgdbIdsResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<GetMediaByIgdbIdsResponse>> Handle(
    GetMediaByIgdbIdsQuery request,
    CancellationToken cancellationToken
  )
  {
    if (request.IgdbIds.Count == 0)
      return Result.Success(new GetMediaByIgdbIdsResponse());

    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql = $"""
      SELECT
        m.public_id AS {nameof(MediaIgdbSummary.PublicId)},
        m.igdb_id AS {nameof(MediaIgdbSummary.IgdbId)},
        m.title   AS {nameof(MediaIgdbSummary.Title)},
        m.year    AS {nameof(MediaIgdbSummary.Year)},
        m.image AS {nameof(MediaIgdbSummary.PosterUrl)}
      FROM movies.media m
      WHERE m.igdb_id = ANY(@IgdbIds)
      ORDER BY m.title ASC;
      """;

    var rows = await connection.QueryAsync<MediaIgdbSummary>(
      new CommandDefinition(
        sql,
        new { IgdbIds = request.IgdbIds.ToArray() },
        cancellationToken: cancellationToken
      )
    );

    return Result.Success(new GetMediaByIgdbIdsResponse { Items = [.. rows] });
  }
}
