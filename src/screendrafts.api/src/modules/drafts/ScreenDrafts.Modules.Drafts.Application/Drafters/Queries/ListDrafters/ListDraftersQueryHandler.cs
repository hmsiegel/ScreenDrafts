namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Queries.ListDrafters;

internal sealed class ListDraftersQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : PaginatedDapperQueryHandler<ListDraftersQuery, DrafterResponse>(dbConnectionFactory),
  IQueryHandler<ListDraftersQuery, PagedResult<DrafterResponse>>
{
  private readonly string _whereClause = PeopleSqlQueryBuilder.BuildWhereClause();

  protected override string GetCountSql() => 
    $"""
      SELECT COUNT(*)
      FROM drafts.drafters d
      INNER JOIN drafts.people p ON p.id = d.person_id
      {_whereClause}
    """;

  protected override string GetDataSql() =>
      $"""
      SELECT
        d.id AS {nameof(DrafterResponse.Id)},
        p.id AS {nameof(DrafterResponse.PersonId)},
        p.first_name AS {nameof(DrafterResponse.FirstName)},
        p.last_name AS {nameof(DrafterResponse.LastName)},
        p.display_name AS {nameof(DrafterResponse.DisplayName)}
        FROM drafts.drafters d
        INNER JOIN drafts.people p ON p.id = d.person_id
      """ + Environment.NewLine + _whereClause + Environment.NewLine +
      """
        {OrderByClause}
        {PaginationClause}
      """;

  protected override IReadOnlySet<string> GetSortableColumns() =>
    new HashSet<string>
    {
      "first_name",
      "last_name",
      "display_name"
    };

  public async Task<Result<PagedResult<DrafterResponse>>> Handle(
    ListDraftersQuery request,
    CancellationToken cancellationToken)
  {
    return await HandleAsync(
      request,
      request.Search,
      request.Sort ?? "first_name",
      request.Dir ?? "asc",
      request.Page,
      request.PageSize,
      cancellationToken);
  }
}
