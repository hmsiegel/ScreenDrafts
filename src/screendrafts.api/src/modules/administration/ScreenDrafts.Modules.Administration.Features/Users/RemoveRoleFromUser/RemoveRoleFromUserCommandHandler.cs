namespace ScreenDrafts.Modules.Administration.Features.Users.RemoveRoleFromUser;

internal sealed class RemoveRoleFromUserCommandHandler(
  IUsersApi usersApi,
  IDbConnectionFactory dbConnectionFactory,
  IEventBus eventBus,
  IDateTimeProvider dateTimeProvider)
  : ICommandHandler<RemoveRoleFromUserCommand, bool>
{
  private readonly IUsersApi _usersApi = usersApi;
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;
  private readonly IEventBus _eventBus = eventBus;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public async Task<Result<bool>> Handle(
    RemoveRoleFromUserCommand request,
    CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    var user = await _usersApi.GetUserByPublicId(request.UserPublicId, cancellationToken);

    if (user is null)
    {
      return Result.Failure<bool>(AdministrationErrors.UserNotFound(request.UserPublicId));
    }

    const string deltaSql =
      $"""
      SELECT rp.permission_code AS {nameof(PermissionRow.PermissionCode)}
      FROM administration.role_permissions rp
      WHERE rp.role_name = @RoleName
        AND rp.permission_code NOT IN (
          SELECT rp2.permission_code
          FROM administration.user_roles ur
          JOIN administration.role_permissions rp2 ON ur.role_name = rp2.role_name
          WHERE ur.user_id = @UserId
            AND ur.role_name <> @RoleName
        )
      """;

    var permissionCodesToRemove = (await connection.QueryAsync<PermissionRow>(
      new CommandDefinition(
        deltaSql,
        new
        {
          request.RoleName,
          user.UserId,
        },
        cancellationToken: cancellationToken)))
        .Select(r => r.PermissionCode)
        .ToList();


    const string deleteSql =
      """
      DELETE FROM administration.user_roles
      WHERE user_id = @UserId
        AND role_name = @RoleName
      """;

    await connection.ExecuteAsync(
      new CommandDefinition(
        deleteSql,
        new
        {
          user.UserId,
          request.RoleName
        },
        cancellationToken: cancellationToken));

    await _eventBus.PublishAsync(
      new UserRoleRemovedIntegrationEvent(
        id: Guid.NewGuid(),
        occurredOnUtc: _dateTimeProvider.UtcNow,
        userId: user.UserId,
        roleName: request.RoleName,
        permissionCodesToRemove: permissionCodesToRemove), 
      cancellationToken);

    return Result.Success(true);
  }

  private sealed record PermissionRow(string PermissionCode);
}


