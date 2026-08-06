namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMediaByExternalIds;

internal sealed class GetMediaByExternalIdsQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<GetMediaByExternalIdsQuery, GetMediaByExternalIdsResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<GetMediaByExternalIdsResponse>> Handle(
    GetMediaByExternalIdsQuery request,
    CancellationToken cancellationToken
  )
  {
    if (request.ExternalIds.Count == 0)
      return Result.Success(new GetMediaByExternalIdsResponse());

    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql = $"""
      SELECT
        m.public_id AS {nameof(MediaExternalSummary.PublicId)},
        m.external_id AS {nameof(MediaExternalSummary.ExternalId)},
        m.title   AS {nameof(MediaExternalSummary.Title)},
        m.year    AS {nameof(MediaExternalSummary.Year)},
        m.image AS {nameof(MediaExternalSummary.PosterUrl)}
      FROM movies.media m
      WHERE m.external_id = ANY(@ExternalIds)
      ORDER BY m.title ASC;
      """;

    var rows = await connection.QueryAsync<MediaExternalSummary>(
      new CommandDefinition(
        sql,
        new { ExternalIds = request.ExternalIds.ToArray() },
        cancellationToken: cancellationToken
      )
    );

    return Result.Success(new GetMediaByExternalIdsResponse { Items = [.. rows] });
  }
}
