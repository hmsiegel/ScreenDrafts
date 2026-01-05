namespace ScreenDrafts.Modules.Drafts.Application.People.Queries.GetPerson;

internal sealed class GetPersonQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<GetPersonQuery, PersonResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<PersonResponse>> Handle(GetPersonQuery request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql =
      $"""
      SELECT
        p.id AS {nameof(PersonResponse.Id)},
        p.first_name AS {nameof(PersonResponse.FirstName)},
        p.last_name AS {nameof(PersonResponse.LastName)},
        p.display_name AS {nameof(PersonResponse.DisplayName)},
        d.id AS {nameof(PersonResponse.DrafterId)},
        h.id AS {nameof(PersonResponse.HostId)},
        CASE WHEN d.id IS NOT NULL THEN TRUE ELSE FALSE END AS {nameof(PersonResponse.IsDrafter)},
        CASE WHEN h.id IS NOT NULL THEN TRUE ELSE FALSE END AS {nameof(PersonResponse.IsHost)}
      FROM drafts.people p
      LEFT JOIN drafts.drafters d ON p.id = d.person_id
      LEFT JOIN drafts.hosts h ON p.id = h.person_id
      WHERE p.id = @PersonId
      """;

    var person = await connection.QuerySingleOrDefaultAsync<PersonResponse>(sql, new { request.PersonId });

    if (person is null)
    {
      return Result.Failure<PersonResponse>(PersonErrors.NotFound(request.PersonId));
    }

    return person;
  }
}
