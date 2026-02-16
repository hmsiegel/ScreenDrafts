namespace ScreenDrafts.Modules.Drafts.Features.Drafters.Get;

internal sealed class GetDrafterQueryHandler(IDbConnectionFactory dbConnectionFactory) : IQueryHandler<GetDrafterQuery, Response>
{

  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<Response>> Handle(GetDrafterQuery request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql =
      $"""
        select
          d.public_id as {nameof(Response.DrafterId)},
          d.person_id as {nameof(Response.PersonId)},
          d.display_name as {nameof(Response.DisplayName)},
          d.is_retired as {nameof(Response.IsRetired)},
          d.retired_on_utc as {nameof(Response.RetiredOnUtc)}
        from
          drafts.drafters d
        where
          d.public_id = @DrafterI d
      """;

    var drafter = await connection.QuerySingleOrDefaultAsync<Response>(new CommandDefinition(
      sql,
      new { request.DrafterId },
      cancellationToken: cancellationToken));

    return drafter is null
      ? Result.Failure<Response>(DrafterErrors.NotFound(request.DrafterId))
      : Result.Success(drafter);
  }
}


