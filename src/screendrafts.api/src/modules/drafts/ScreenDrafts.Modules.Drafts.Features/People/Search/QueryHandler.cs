namespace ScreenDrafts.Modules.Drafts.Features.People.Search;

internal sealed class QueryHandler(IDbConnectionFactory dbConnectionFactory) : IQueryHandler<Query, PeopleSearchResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<PeopleSearchResponse>> Handle(Query request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql =
      $"""
        SELECT 
          p.public_id as {nameof(PersonSearchItem.PublicId)},
          p.display_name as {nameof(PersonSearchItem.DisplayName)}
        FROM
          drafts.people p
        WHERE
          p.display_name ILIKE '%' || @Search || '%'
          OR p.first_name ILIKE '%' || @Search || '%'
          OR p.last_name ILIKE '%' || @Search || '%'
        ORDER BY
          CASE WHEN p.display_name ILIKE @Search || '%' THEN 0 ELSE 1 END,
          p.display_name
        LIMIT @Limit;
      """;

    var items = await connection.QueryAsync<PersonSearchItem>(new CommandDefinition(
      sql,
      parameters: new
      {
        request.Search,
        request.Limit
      },
      cancellationToken: cancellationToken));

    var response = new PeopleSearchResponse(Items: [.. items]);

    return Result.Success(response);
  }
}
