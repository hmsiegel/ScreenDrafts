namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.ListUpcomingDrafts;

internal sealed class ListUpcomingDraftsQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<ListUpcomingDraftsQuery, IReadOnlyList<DraftResponse>>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<IReadOnlyList<DraftResponse>>> Handle(ListUpcomingDraftsQuery request, CancellationToken cancellationToken)
  {
    await using DbConnection connection = await _dbConnectionFactory.OpenConnectionAsync();

    const string sql =
      $"""
            SELECT
              d.id AS {nameof(DraftResponse.Id)},
              d.title AS {nameof(DraftResponse.Title)},
              array_agg(rd.release_date ORDER BY rd.release_date) AS {nameof(DraftResponse.RawReleaseDates)}
            FROM drafts.drafts d
            JOIN drafts.draft_release_date rd ON d.id = rd.draft_id
            WHERE rd.release_date > @Today
            GROUP BY d.id, d.title
            ORDER BY MAX(rd.release_date) ASC
            """;

    var drafts = (await connection.QueryAsync<DraftResponse>(sql, new { DateTime.Today })).ToList();
    foreach (var draft in drafts)
    {
      draft.PopulateReleaseDatesFromRaw();
    }

    return drafts;
  }
}
