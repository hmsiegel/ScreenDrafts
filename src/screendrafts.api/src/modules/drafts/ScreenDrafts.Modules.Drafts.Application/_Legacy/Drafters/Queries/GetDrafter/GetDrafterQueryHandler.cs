namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafters.Queries.GetDrafter;

internal sealed class GetDrafterQueryHandler(IDbConnectionFactory dbConnectionFactory) : IQueryHandler<GetDrafterQuery, DrafterResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<DrafterResponse>> Handle(GetDrafterQuery request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql =
      $"""
          SELECT
            d.id AS {nameof(DrafterResponse.Id)},
            d.person_id AS {nameof(DrafterResponse.PersonId)},
            p.first_name AS {nameof(DrafterResponse.FirstName)},
            p.last_name AS {nameof(DrafterResponse.LastName)},
            p.display_name AS {nameof(DrafterResponse.DisplayName)}
          FROM drafts.drafters d
          INNER JOIN drafts.people p ON d.person_id = p.id
          WHERE d.id = @DrafterId
         """;

    var drafter = await connection.QuerySingleOrDefaultAsync<DrafterResponse>(sql, request);

    if (drafter is null)
    {
      return Result.Failure<DrafterResponse>(DrafterErrors.NotFound(request.DrafterId));
    }

    return drafter;
  }
}
