namespace ScreenDrafts.Modules.Users.Features.Users.GetUserPermissions;

internal sealed class QueryHandler(IDbConnectionFactory dbConnectionFactory) : IQueryHandler<Query, PermissionsResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<PermissionsResponse>> Handle(Query request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql =
      $"""
      SELECT DISTINCT
        u.id AS {nameof(UserPermission.UserId)},
        rp.permission_code AS {nameof(UserPermission.Permission)}
      FROM users.users u
      JOIN users.user_roles ur ON ur.user_id = u.id
      JOIN users.role_permissions rp ON rp.role_name = ur.role_name
      WHERE u.identity_id = @IdentityId
      """;

    var permissions = (await connection.QueryAsync<UserPermission>(sql, request)).AsList();

    if (permissions.Count == 0)
    {
      return Result.Failure<PermissionsResponse>(UserErrors.NotFound(request.IdentityId));
    }

    return new PermissionsResponse(permissions[0].UserId, permissions.Select(p => p.Permission).ToHashSet());
  }

  internal sealed class UserPermission
  {
    internal Guid UserId { get; init; }

    internal string Permission { get; init; } = string.Empty;
  }
}
