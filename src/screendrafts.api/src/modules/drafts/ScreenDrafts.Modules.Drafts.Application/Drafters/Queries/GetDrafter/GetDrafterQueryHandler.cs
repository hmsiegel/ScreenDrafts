namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Queries.GetDrafter;

internal sealed class GetDrafterQueryHandler(IDbConnectionFactory dbConnectionFactory) : IQueryHandler<GetDrafterQuery, DrafterResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<DrafterResponse>> Handle(GetDrafterQuery request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync();

    const string sql =
      $"""
          SELECT
            id AS {nameof(Drafter.Id)},
            name AS {nameof(Drafter.Name)},
            user_id AS {nameof(Drafter.UserId)}
          FROM drafts.drafters
          WHERE id = @DrafterId
         """;

    var drafter = await connection.QuerySingleOrDefaultAsync<DrafterResponse>(sql, request);

    if (drafter is null)
    {
      return Result.Failure<DrafterResponse>(DrafterErrors.NotFound(request.DrafterId));
    }

    return drafter;
  }
}
