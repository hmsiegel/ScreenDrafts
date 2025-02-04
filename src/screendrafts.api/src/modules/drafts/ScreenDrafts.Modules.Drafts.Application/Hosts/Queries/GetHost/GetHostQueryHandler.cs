namespace ScreenDrafts.Modules.Drafts.Application.Hosts.Queries.GetHost;

internal sealed class GetHostQueryHandler(IDbConnectionFactory dbConnectionFactory) : IQueryHandler<GetHostQuery, HostResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<HostResponse>> Handle(GetHostQuery request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync();

    const string sql =
      $"""
          SELECT
            id AS {nameof(HostResponse.Id)},
            host_name AS {nameof(HostResponse.Name)}
          FROM drafts.hosts
          WHERE id = @HostId
         """;

    var host = await connection.QuerySingleOrDefaultAsync<HostResponse>(sql, request);

    if (host is null)
    {
      return Result.Failure<HostResponse>(HostErrors.NotFound(request.HostId));
    }

    return host;
  }
}
