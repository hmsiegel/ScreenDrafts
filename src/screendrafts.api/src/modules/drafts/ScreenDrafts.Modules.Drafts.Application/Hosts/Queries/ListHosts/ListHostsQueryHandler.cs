namespace ScreenDrafts.Modules.Drafts.Application.Hosts.Queries.ListHosts;

internal sealed class ListHostsQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<ListHostsQuery, IReadOnlyCollection<HostResponse>>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<IReadOnlyCollection<HostResponse>>> Handle(
    ListHostsQuery request,
    CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync();

    const string sql =
      $"""
        SELECT
          id AS {nameof(HostResponse.Id)},
          host_name AS {nameof(HostResponse.Name)}
        FROM drafts.hosts
        """;

    var hosts = await connection.QueryAsync<HostResponse>(sql, request);

    return hosts.ToList();
  }
}
