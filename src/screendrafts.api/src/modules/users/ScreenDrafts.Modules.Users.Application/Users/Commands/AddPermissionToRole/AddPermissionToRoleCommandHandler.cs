namespace ScreenDrafts.Modules.Users.Application.Users.Commands.AddPermissionToRole;

internal sealed class AddPermissionToRoleCommandHandler(IDbConnectionFactory dbConnectionFactory) : ICommandHandler<AddPermissionToRoleCommand, bool>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<bool>> Handle(AddPermissionToRoleCommand request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync();

    const string getSql =
      $"""
        SELECT DISTINCT
          rp.role_name AS {nameof(RolePermission.RoleName)},
          rp.permission_code AS {nameof(RolePermission.PermissionCode)}
        FROM users.role_permissions rp
      """;

    var permissions = (await connection.QueryAsync<RolePermission>(getSql)).AsList();

    if (permissions.Any(p => p.RoleName == request.Role && p.PermissionCode == request.Permission))
    {
      return Result.Failure<bool>(UserErrors.PermissionAlreadyExists(request.Permission));
    }

    const string sql =
      $"""
        INSERT INTO users.role_permissions (role_name, permission_code)
        VALUES (@Role, @Permission)
      """;

    await connection.ExecuteAsync(sql, request);

    return true;
  }

  internal sealed class RolePermission
  {
    internal string RoleName { get; init; } = string.Empty;
    internal string PermissionCode { get; init; } = string.Empty;
  }
}
