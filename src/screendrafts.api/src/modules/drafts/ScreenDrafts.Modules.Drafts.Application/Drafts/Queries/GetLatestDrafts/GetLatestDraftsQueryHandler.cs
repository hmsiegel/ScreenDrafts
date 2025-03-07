﻿namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetLatestDrafts;

internal sealed class GetLatestDraftsQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<GetLatestDraftsQuery, IReadOnlyList<DraftResponse>>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;
  public async Task<Result<IReadOnlyList<DraftResponse>>> Handle(GetLatestDraftsQuery request, CancellationToken cancellationToken)
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
            WHERE d.draft_status = 3
            GROUP BY d.id, d.title
            ORDER BY MAX(rd.release_date) DESC
            LIMIT 5
            """;

    List<DraftResponse> drafts = [.. await connection.QueryAsync<DraftResponse>(sql)];
    foreach (var draft in drafts)
    {
      draft.PopulateReleaseDatesFromRaw();
    }

    return drafts;
  }
}
