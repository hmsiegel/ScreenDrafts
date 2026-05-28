namespace ScreenDrafts.Modules.Administration.Features.Users.GetRoles;

internal sealed class GetRolesQueryHandler(IDbConnectionFactory connectionFactory)
  : IQueryHandler<GetRolesQuery, GetRolesResponse>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

  public async Task<Result<GetRolesResponse>> Handle(
    GetRolesQuery request,
    CancellationToken cancellationToken
  )
  {
    using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql = $"""
      SELECT r.name AS {nameof(RoleRow.Name)}
      FROM administration.roles r
      ORDER BY r.name
      """;

    var roles = (
      await connection.QueryAsync<RoleRow>(
        new CommandDefinition(sql, cancellationToken: cancellationToken)
      )
    )
      .Select(r => r.Name)
      .ToList();

    return Result.Success(new GetRolesResponse { Roles = roles });
  }

  private sealed record RoleRow(string Name);
}
