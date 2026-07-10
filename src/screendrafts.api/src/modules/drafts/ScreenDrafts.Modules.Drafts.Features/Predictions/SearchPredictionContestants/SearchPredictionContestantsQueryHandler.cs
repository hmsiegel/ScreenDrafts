namespace ScreenDrafts.Modules.Drafts.Features.Predictions.SearchPredictionContestants;

internal sealed class SearchPredictionContestantsQueryHandler(
  IDbConnectionFactory connectionFactory
) : IQueryHandler<SearchPredictionContestantsQuery, SearchPredictionContestantsResponse>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

  public async Task<Result<SearchPredictionContestantsResponse>> Handle(
    SearchPredictionContestantsQuery request,
    CancellationToken cancellationToken
  )
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql = """
      SELECT
        public_id    AS PublicId,
        display_name AS DisplayName
      FROM drafts.prediction_contestants
      WHERE is_active = true
        AND (@Name IS NULL OR display_name ILIKE '%' || @Name || '%')
      ORDER BY display_name
      LIMIT @PageSize;
      """;

    var rows = (
      await connection.QueryAsync<ContestantSearchResultResponse>(
        new CommandDefinition(
          sql,
          new { request.Name, request.PageSize },
          cancellationToken: cancellationToken
        )
      )
    ).ToList();

    return Result.Success(new SearchPredictionContestantsResponse { Items = rows });
  }
}
