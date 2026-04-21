namespace ScreenDrafts.Modules.Administration.Features.Users.ListPermissions;

internal sealed class ListPermissionsQueryHandler(IDbConnectionFactory connectionFactory)
    : IQueryHandler<ListPermissionsQuery, ListPermissionsResponse>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

  public async Task<Result<ListPermissionsResponse>> Handle(ListPermissionsQuery request, CancellationToken cancellationToken)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql =
      $"""
      SELECT code AS {nameof(PermissionsRow.Code)}
      FROM administration.permissions
      ORDER BY code
      """;

    var permissions = await connection.QueryAsync<PermissionsRow>(
      new CommandDefinition(
        sql,
        cancellationToken: cancellationToken));

    var permissionsList = permissions.Select(p => p.Code).ToList();

    return new ListPermissionsResponse
    {
      Permissions = permissionsList
    };
  }

  private sealed record PermissionsRow(string Code);
}
