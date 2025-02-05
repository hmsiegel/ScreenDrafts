namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Queries.ListDrafters;

internal sealed class ListDraftersQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<ListDraftersQuery, IReadOnlyCollection<DrafterResponse>>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<IReadOnlyCollection<DrafterResponse>>> Handle(
    ListDraftersQuery request,
    CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync();

    const string sql =
      $"""
        SELECT
          id AS {nameof(DrafterResponse.Id)},
          name AS {nameof(DrafterResponse.Name)}
        FROM drafts.drafters
        """;

    var drafters = await connection.QueryAsync<DrafterResponse>(sql, request);

    return drafters.ToList();
  }
}
