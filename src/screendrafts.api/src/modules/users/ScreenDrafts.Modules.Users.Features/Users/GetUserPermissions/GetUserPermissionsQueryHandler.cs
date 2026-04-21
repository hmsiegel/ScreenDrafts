namespace ScreenDrafts.Modules.Users.Features.Users.GetUserPermissions;

internal sealed class GetUserPermissionsQueryHandler(IDbConnectionFactory dbConnectionFactory) : IQueryHandler<GetUserPermissionsQuery, PermissionsResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<PermissionsResponse>> Handle(GetUserPermissionsQuery request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql =
      $"""
      SELECT DISTINCT
        u.id AS {nameof(UserPermission.UserId)},
        u.public_id AS {nameof(UserPermission.PublicId)},
        up.permission_code AS {nameof(UserPermission.Permission)}
      FROM users.users u
      JOIN users.user_permissions up ON up.user_id = u.id
      WHERE u.identity_id = @IdentityId
      """;

    var permissions = (await connection.QueryAsync<UserPermission>(sql, request)).AsList();

    if (permissions.Count == 0)
    {
      return Result.Failure<PermissionsResponse>(UserErrors.NotFound(request.IdentityId));
    }

    return new PermissionsResponse(
      permissions[0].UserId,
      permissions[0].PublicId,
      [.. permissions.Select(p => p.Permission)]);
  }

  internal sealed class UserPermission
  {
    internal Guid UserId { get; init; }

    internal string PublicId { get; init; } = string.Empty;

    internal string Permission { get; init; } = string.Empty;
  }
}
