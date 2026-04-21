namespace ScreenDrafts.Modules.Administration.Features.Users.GetPermissionByCode;

internal sealed class GetPermissionByCodeQueryHandler(IDbConnectionFactory connectionFactory)
    : IQueryHandler<GetPermissionByCodeQuery, PermissionResponse>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

  public async Task<Result<PermissionResponse>> Handle(GetPermissionByCodeQuery request, CancellationToken cancellationToken)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql =
      $"""
      SELECT
        p.code AS {nameof(PermissionResponse.Code)},
        COALESCE(
          ARRAY_AGG(rp.role_name ORDER BY rp.role_name)
          FILTER (WHERE rp.role_name IS NOT NULL),
          ARRAY[]::varchar[]
          ) AS {nameof(PermissionResponse.Roles)}
      FROM administration.permissions p
      LEFT JOIN administration.role_permissions rp ON p.code = rp.permission_code
      WHERE p.code = @Code
      GROUP BY p.code
      """;

    var permission = await connection.QuerySingleOrDefaultAsync<PermissionResponse>(
      new CommandDefinition(
        sql,
        new { request.Code },
        cancellationToken: cancellationToken));

    if (permission is null)
    {
      return Result.Failure<PermissionResponse>(AdministrationErrors.PermissionNotFound(request.Code));
    }

    return Result.Success(permission);
  }
}


