namespace ScreenDrafts.Modules.Drafts.Features.People.Get;

internal sealed class QueryHandler(IDbConnectionFactory dbConnectionFactory) : IQueryHandler<Query, PersonResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<PersonResponse>> Handle(Query request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql =
      $"""
        SELECT
          p.user_id as {nameof(PersonResponse.UserId)},
          p.public_id as {nameof(PersonResponse.PublicId)},
          p.first_name as {nameof(PersonResponse.FirstName)},
          p.last_name as {nameof(PersonResponse.LastName)},
          p.display_name as {nameof(PersonResponse.DisplayName)},
          d.public_id as {nameof(PersonResponse.DrafterPublicId)},
          h.public_id as {nameof(PersonResponse.HostPublicId)},
        FROM
           drafts.people p
        LEFT JOIN drafts.drafters d ON p.id = d.person_id
        LEFT JOIN drafts.hosts h ON p.id = h.person_id
        WHERE p.public_id = @PublicId
      """;

    var person = await connection.QuerySingleOrDefaultAsync<PersonResponse>(new CommandDefinition(
      sql,
      new { request.PublicId },
      cancellationToken: cancellationToken));

    if (person is null)
    {
      return Result.Failure<PersonResponse>(PersonErrors.NotFound(request.PublicId));
    }

    return person;
  }
}
