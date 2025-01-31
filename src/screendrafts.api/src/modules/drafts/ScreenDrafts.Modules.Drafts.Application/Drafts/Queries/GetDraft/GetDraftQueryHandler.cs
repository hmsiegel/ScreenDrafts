namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetDraft;

internal sealed class GetDraftQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<GetDraftQuery, DraftResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<DraftResponse>> Handle(GetDraftQuery request, CancellationToken cancellationToken)
  {
    await using DbConnection connection = await _dbConnectionFactory.OpenConnectionAsync();

    const string sql =
      $"""
            SELECT
              id AS {nameof(Draft.Id)},
              title AS {nameof(Draft.Title)},
              draft_type AS {nameof(Draft.DraftType)},
              total_picks AS {nameof(Draft.TotalPicks)},
              total_drafters AS {nameof(Draft.TotalDrafters)},
              total_hosts AS {nameof(Draft.TotalHosts)},
              draft_status AS {nameof(Draft.DraftStatus)}
            FROM drafts.drafts
            WHERE id = @DraftId
            """;

    DraftResponse? draft = await connection.QuerySingleOrDefaultAsync<DraftResponse>(sql, request);

    if (draft is null)
    {
      return Result.Failure<DraftResponse>(DraftErrors.NotFound(request.DraftId));
    }

    return draft;
  }
}
