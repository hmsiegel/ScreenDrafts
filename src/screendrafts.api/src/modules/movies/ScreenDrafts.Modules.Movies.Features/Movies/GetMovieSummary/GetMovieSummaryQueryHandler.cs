namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMovieSummary;

internal sealed class GetMovieSummaryQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<GetMovieSummaryQuery, GetMovieSummaryResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<GetMovieSummaryResponse>> Handle(GetMovieSummaryQuery request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql =
      $"""
      SELECT 
        m.imdb_id AS {nameof(GetMovieSummaryResponse.ImdbId)},
        m.title AS {nameof(GetMovieSummaryResponse.Title)},
        m.year AS {nameof(GetMovieSummaryResponse.Year)},
        m.image AS {nameof(GetMovieSummaryResponse.Image)},
        m.plot AS {nameof(GetMovieSummaryResponse.Plot)}
      FROM movies.movies m
      WHERE m.imdb_id = @{nameof(GetMovieSummaryQuery.ImdbId)}
      """;

    var result = await connection.QuerySingleOrDefaultAsync<GetMovieSummaryResponse>(
      new CommandDefinition(
        sql,
        new { request.ImdbId },
        cancellationToken: cancellationToken));

    if (result is null)
    {
      return Result.Failure<GetMovieSummaryResponse>(MovieErrors.MovieNotFound(request.ImdbId)); 
    }

    return Result.Success(result);
  }
}
