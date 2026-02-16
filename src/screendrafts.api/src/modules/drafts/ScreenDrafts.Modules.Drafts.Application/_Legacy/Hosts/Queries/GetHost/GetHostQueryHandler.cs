namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Hosts.Queries.GetHost;

internal sealed class GetHostQueryHandler(IDbConnectionFactory dbConnectionFactory) : IQueryHandler<GetHostQuery, HostResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<HostResponse>> Handle(GetHostQuery request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql =
      $"""
          SELECT
            h.id AS {nameof(HostResponse.Id)},
            h.person_id As {nameof(HostResponse.PersonId)},
            p.first_name AS {nameof(HostResponse.FirstName)},
            p.last_name AS {nameof(HostResponse.LastName)},
            p.display_name AS {nameof(HostResponse.DisplayName)}
          FROM drafts.hosts h
          INNER JOIN drafts.people p ON p.id = h.person_id
          WHERE h.id = @HostId
         """;

    var host = await connection.QuerySingleOrDefaultAsync<HostResponse>(sql, request);

    if (host is null)
    {
      return Result.Failure<HostResponse>(HostErrors.NotFound(request.HostId));
    }

    return host;
  }
}
