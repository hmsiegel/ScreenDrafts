namespace ScreenDrafts.Modules.Drafts.Features.People.List;

internal sealed class ListPeopleQueryHandler(IDbConnectionFactory connectionFactory) : IQueryHandler<ListPeopleQuery, PeopleCollectionResponse>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

  public async Task<Result<PeopleCollectionResponse>> Handle(ListPeopleQuery ListPeopleRequest, CancellationToken cancellationToken)
  {
    var r = ListPeopleRequest.ListPeopleRequest;

    var page = r.Page;
    var pageSize = r.PageSize;
    var offset = (page - 1) * pageSize;

    var sortExpr = r.Sort.ToLowerInvariant() switch
    {
      "firstname" => "p.first_name",
      "lastname" => "p.last_name",
      "publicId" => "p.public_id",
      _ => "p.display_name"
    };

    var dir = r.Dir.Equals("desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";

    var args = new
    {
      Name = string.IsNullOrWhiteSpace(r.Name) ? null : $"%{r.Name.Trim()}%",
      r.HasDrafter,
      r.HasHost,
      PageSize = pageSize,
      Offset = offset
    };

    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    const string countSql =
      $"""
        SELECT COUNT(*)::int
        FROM drafts.people p
        WHERE
          (@Name IS NULL OR (
          p.first_name ILIKE '%' || @Name || '%' OR
          p.last_name ILIKE '%' || @Name || '%' OR
          p.display_name ILIKE '%' || @Name || '%'))
        AND (
          @HasDrafter IS NULL OR
          @HasDrafter = EXISTS (
            SELECT 1
            FROM drafts.drafters d
            WHERE d.person_id = p.id
          )
        )
        AND (
          @HasHost IS NULL OR
          @HasHost = EXISTS (
            SELECT 1
            FROM drafts.hosts h
            WHERE h.person_id = p.id
          )
        )
      """;

    var totalCount = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
      countSql,
      parameters: args,
      cancellationToken: cancellationToken));

    if (totalCount == 0)
    {
      return Result.Success(new PeopleCollectionResponse(new PagedResult<PersonResponse>
      {
        Items = [],
        TotalCount = 0,
        Page = page,
        PageSize = pageSize
      }));
    }

    var pageSql = $"""
    SELECT
      p.public_id    AS {nameof(PersonResponse.PublicId)},
      p.first_name   AS {nameof(PersonResponse.FirstName)},
      p.last_name    AS {nameof(PersonResponse.LastName)},
      p.display_name AS {nameof(PersonResponse.DisplayName)},
      (
        SELECT d.public_id FROM drafts.drafters d WHERE d.person_id = p.id LIMIT 1
      ) AS {nameof(PersonResponse.DrafterPublicId)},
      (
        SELECT h.public_id FROM drafts.hosts h WHERE h.person_id = p.id LIMIT 1
      ) AS {nameof(PersonResponse.HostPublicId)}
    FROM drafts.people p
    WHERE
      (@Name IS NULL OR (
        p.first_name ILIKE '%' || @Name || '%' OR
        p.last_name ILIKE '%' || @Name || '%' OR
        p.display_name ILIKE '%' || @Name || '%'
      ))
      AND (
        @HasDrafter IS NULL OR
        @HasDrafter = EXISTS (SELECT 1 FROM drafts.drafters d WHERE d.person_id = p.id)
      )
      AND (
        @HasHost IS NULL OR
        @HasHost = EXISTS (SELECT 1 FROM drafts.hosts h WHERE h.person_id = p.id)
      )
    ORDER BY {sortExpr} {dir}, p.public_id ASC
    LIMIT @PageSize OFFSET @Offset;
  """;

    var items = (await connection.QueryAsync<PersonResponse>(new CommandDefinition(
      pageSql,
      parameters: args,
      cancellationToken: cancellationToken))).ToList();

    return Result.Success(new PeopleCollectionResponse(new PagedResult<PersonResponse>
    {
      Items = new Collection<PersonResponse>(items),
      TotalCount = totalCount,
      Page = page,
      PageSize = pageSize
    }));
  }
}





