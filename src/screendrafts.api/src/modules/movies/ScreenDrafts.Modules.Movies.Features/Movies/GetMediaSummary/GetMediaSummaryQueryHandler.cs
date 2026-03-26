namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMediaSummary;

internal sealed class GetMediaSummaryQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<GetMediaSummaryQuery, GetMediaSummaryResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<GetMediaSummaryResponse>> Handle(GetMediaSummaryQuery request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql =
      $"""
      SELECT 
        m.public_id AS {nameof(GetMediaSummaryResponse.PublicId)},
        m.imdb_id AS {nameof(GetMediaSummaryResponse.ImdbId)},
        m.tmdb_id AS {nameof(GetMediaSummaryResponse.TmdbId)},
        m.igdb_id AS {nameof(GetMediaSummaryResponse.IgdbID)},
        m.title AS {nameof(GetMediaSummaryResponse.Title)},
        m.year AS {nameof(GetMediaSummaryResponse.Year)},
        m.image AS {nameof(GetMediaSummaryResponse.Image)},
        m.plot AS {nameof(GetMediaSummaryResponse.Plot)},
        m.media_type AS {nameof(GetMediaSummaryResponse.MediaType)}
      FROM movies.media m
      WHERE m.public_id = @{nameof(GetMediaSummaryQuery.PublicId)}
      """;

    var result = await connection.QuerySingleOrDefaultAsync<GetMediaSummaryResponse>(
      new CommandDefinition(
        sql,
        new { request.PublicId },
        cancellationToken: cancellationToken));

    if (result is null)
    {
      return Result.Failure<GetMediaSummaryResponse>(MediaErrors.MediaNotFound(request.PublicId)); 
    }

    return Result.Success(result);
  }
}
