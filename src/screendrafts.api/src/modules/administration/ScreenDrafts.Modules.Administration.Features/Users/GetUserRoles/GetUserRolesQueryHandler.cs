namespace ScreenDrafts.Modules.Administration.Features.Users.GetUserRoles;

internal sealed class GetUserRolesQueryHandler(IDbConnectionFactory connectionFactory, IUsersApi usersApi)
  : IQueryHandler<GetUserRolesQuery, GetUserRolesResponse>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
  private readonly IUsersApi _usersApi = usersApi;
  public async Task<Result<GetUserRolesResponse>> Handle(
    GetUserRolesQuery request,
    CancellationToken cancellationToken)
  {
    var user = await _usersApi.GetUserByPublicId(request.PublicId, cancellationToken);

    if (user is null)
    {
      return Result.Failure<GetUserRolesResponse>(AdministrationErrors.UserNotFound(request.PublicId));
    }

    using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql =
      $"""
      SELECT ur.role_name AS {nameof(RoleRow.RoleName)}
      FROM administration.user_roles ur
      WHERE ur.user_id = @UserId
      """;

    var roles = (await connection.QueryAsync<RoleRow>(
      new CommandDefinition(
        sql,
        new { user.UserId },
        cancellationToken: cancellationToken)))
        .Select(r => r.RoleName)
        .ToList();

    if (roles.Count == 0)
    {
      return Result.Failure<GetUserRolesResponse>(AdministrationErrors.UserHasNoRoles(user.UserId));
    }

    return Result.Success(new GetUserRolesResponse { Roles = roles });
  }

  private sealed record RoleRow(string RoleName);
}
