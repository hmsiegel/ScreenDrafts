namespace ScreenDrafts.Modules.Drafts.Features.Drafters.Get;

internal sealed class GetDrafterQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<GetDrafterQuery, GetDrafterResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<GetDrafterResponse>> Handle(
    GetDrafterQuery request,
    CancellationToken cancellationToken
  )
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql = $"""
        select
          d.public_id as {nameof(GetDrafterResponse.DrafterId)},
          p.public_id as {nameof(GetDrafterResponse.PersonId)},
          p.display_name as {nameof(GetDrafterResponse.DisplayName)},
          d.is_retired as {nameof(GetDrafterResponse.IsRetired)},
          d.retired_at_utc as {nameof(GetDrafterResponse.RetiredOnUtc)}
        from
          drafts.drafters d
        left join
          drafts.people p on d.person_id = p.id
        where
          d.public_id = @DrafterId
      """;

    var drafter = await connection.QuerySingleOrDefaultAsync<GetDrafterResponse>(
      new CommandDefinition(sql, new { request.DrafterId }, cancellationToken: cancellationToken)
    );

    return drafter is null
      ? Result.Failure<GetDrafterResponse>(DrafterErrors.NotFound(request.DrafterId))
      : Result.Success(drafter);
  }
}
