namespace ScreenDrafts.Modules.Drafts.Application.Hosts.Queries.ListHosts;

internal sealed class ListHostsQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : PaginatedDapperQueryHandler<ListHostsQuery, HostResponse>(dbConnectionFactory),
  IQueryHandler<ListHostsQuery, PagedResult<HostResponse>>
{
  private readonly string _whereClause = PeopleSqlQueryBuilder.BuildWhereClause();

  protected override string GetCountSql() => 
    $"""
      SELECT COUNT(*)
      FROM drafts.hosts h
      INNER JOIN drafts.people p ON p.id = h.person_id
      {_whereClause}
    """;

  protected override string GetDataSql() =>
      $"""
      SELECT
        h.id AS {nameof(HostResponse.Id)},
        p.id AS {nameof(HostResponse.PersonId)},
        p.first_name AS {nameof(HostResponse.FirstName)},
        p.last_name AS {nameof(HostResponse.LastName)},
        p.display_name AS {nameof(HostResponse.DisplayName)}
        FROM drafts.hosts h
        INNER JOIN drafts.people p ON p.id = h.person_id
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

  public async Task<Result<PagedResult<HostResponse>>> Handle(
    ListHostsQuery request,
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
