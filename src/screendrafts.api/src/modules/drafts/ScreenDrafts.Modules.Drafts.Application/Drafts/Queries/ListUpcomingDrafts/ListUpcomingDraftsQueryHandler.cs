namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.ListUpcomingDrafts;

internal sealed class ListUpcomingDraftsQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<ListUpcomingDraftsQuery, IReadOnlyList<UpcomingDraftResponse>>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<IReadOnlyList<UpcomingDraftResponse>>> Handle(ListUpcomingDraftsQuery request, CancellationToken cancellationToken)
  {
    await using DbConnection connection = await _dbConnectionFactory.OpenConnectionAsync();

    const string sql =
      $"""
            SELECT
              id AS {nameof(UpcomingDraftResponse.DraftId)},
              title AS {nameof(UpcomingDraftResponse.Title)},
              release_date AS {nameof(UpcomingDraftResponse.ReleaseDate)}
            FROM drafts.drafts
            WHERE release_date > @Today
            ORDER BY release_date ASC
            """;

    var drafts = (await connection.QueryAsync<UpcomingDraftResponse>(sql, new { DateTime.Today })).ToList();

    return drafts;
  }
}
