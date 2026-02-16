namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Queries.GetCommissionerOverridesByDraft;

internal sealed class GetCommissionerOverridesByDraftQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<GetCommissionerOverridesByDraftQuery, List<CommissionerOverrideResponse>>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<List<CommissionerOverrideResponse>>> Handle(GetCommissionerOverridesByDraftQuery request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql = $"""
      SELECT
        co.id AS {nameof(CommissionerOverrideResponse.Id)},
        co.pick_id AS {nameof(CommissionerOverrideResponse.PickId)},
        p.position AS {nameof(CommissionerOverrideResponse.Position)},
        p.movie_id AS {nameof (CommissionerOverrideResponse.MovieId)},
        p.drafter_id AS {nameof(CommissionerOverrideResponse.DrafterId)}
      FROM drafts.commissioner_overrides co
      INNER JOIN drafts.picks p ON co.pick_id = p.id
      WHERE p.draft_id = @DraftId
      """;

    List<CommissionerOverrideResponse> commissionerOverrides = [.. await connection.QueryAsync<CommissionerOverrideResponse>(sql, request)];

    if (commissionerOverrides.Count == 0)
    {
      return Result.Failure<List<CommissionerOverrideResponse>>(DraftErrors.CommissionerOverridesNotFound);
    }

    return commissionerOverrides;
  }
}
