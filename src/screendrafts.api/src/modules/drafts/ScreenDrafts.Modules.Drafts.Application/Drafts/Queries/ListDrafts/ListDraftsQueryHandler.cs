namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.ListDrafts;

internal sealed class ListDraftsQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<ListDraftsQuery, IReadOnlyCollection<DraftResponse>>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<IReadOnlyCollection<DraftResponse>>> Handle(
    ListDraftsQuery request,
    CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync();

    const string sql =
      $"""
        SELECT
          id AS {nameof(DraftResponse.Id)},
          title AS {nameof(DraftResponse.Title)},
          draft_type AS {nameof(DraftResponse.DraftType)},
          total_picks AS {nameof(DraftResponse.TotalPicks)},
          total_drafters AS {nameof(DraftResponse.TotalDrafters)},
          total_hosts AS {nameof(DraftResponse.TotalHosts)},
          draft_status AS {nameof(DraftResponse.DraftStatus)}
        FROM drafts.drafts
        """;

    List<DraftResponse> drafts = (await connection.QueryAsync<DraftResponse>(sql, request)).AsList();

    return Result.Success<IReadOnlyCollection<DraftResponse>>(drafts);
  }
}
