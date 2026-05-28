namespace ScreenDrafts.Modules.Administration.Features.Users.GetRolePermissions;

internal sealed class GetRolePermissionsQueryHandler(IDbConnectionFactory connectionFactory)
  : IQueryHandler<GetRolePermissionsQuery, GetRolePermissionsResponse>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

  public async Task<Result<GetRolePermissionsResponse>> Handle(
    GetRolePermissionsQuery request,
    CancellationToken cancellationToken
  )
  {
    using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql = $"""
      SELECT rp.permission_code AS {nameof(PermissionRow.PermissionCode)}
      FROM administration.role_permissions rp
      WHERE rp.role_name = @RoleName
      ORDER BY rp.permission_code
      """;

    var permissions = (
      await connection.QueryAsync<PermissionRow>(
        new CommandDefinition(sql, new { request.RoleName }, cancellationToken: cancellationToken)
      )
    )
      .Select(p => p.PermissionCode)
      .ToList();

    return Result.Success(new GetRolePermissionsResponse { Permissions = permissions });
  }

  private sealed record PermissionRow(string PermissionCode);
}
