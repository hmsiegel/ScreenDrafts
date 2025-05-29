namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.ListUpcomingDrafts;

internal sealed class ListUpcomingDraftsQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<ListUpcomingDraftsQuery, IReadOnlyList<UpcomingDraftDto>>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<IReadOnlyList<UpcomingDraftDto>>> Handle(ListUpcomingDraftsQuery request, CancellationToken cancellationToken)
  {
    await using DbConnection connection = await _dbConnectionFactory.OpenConnectionAsync();

    const string sql =
      $"""
            SELECT
              d.id AS {nameof(UpcomingDraftDto.Id)},
              d.title AS {nameof(UpcomingDraftDto.Title)},
              COALESCE(
                array_agg(rd.release_date ORDER BY rd.release_date)
                FILTER (WHERE rd.release_date IS NOT NULL
                  AND rd.release_date > @Today),
                array[]::date[]
              ) AS {nameof(UpcomingDraftDto.ReleaseDates)}
            FROM drafts.drafts d
            LEFT JOIN drafts.draft_release_date rd ON d.id = rd.draft_id
            WHERE rd.release_date > @Today OR rd.release_date IS NULL
            GROUP BY d.id, d.title
            ORDER BY MIN(rd.release_date) NULLS LAST;
            """;

    var drafts = (await connection.QueryAsync<UpcomingDraftDto>(sql, new { DateTime.Today })).ToList();

    return drafts;
  }
}
