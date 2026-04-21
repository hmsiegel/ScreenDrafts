namespace ScreenDrafts.Modules.Administration.Features.Users.AddRoleToUser;

internal sealed class AddRoleToUserCommandHandler(
  IDbConnectionFactory dbConnectionFactory,
  IEventBus eventBus,
  IDateTimeProvider dateTimeProvider,
  IUsersApi usersApi)
  : ICommandHandler<AddRoleToUserCommand, bool>
{
  private readonly IUsersApi _usersApi = usersApi;
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;
  private readonly IEventBus _eventBus = eventBus;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public async Task<Result<bool>> Handle(AddRoleToUserCommand command, CancellationToken cancellationToken)
  {
    var user = await _usersApi.GetUserByPublicId(command.UserPublicId, cancellationToken);

    if (user is null)
    {
      return Result.Failure<bool>(AdministrationErrors.UserNotFound(command.UserPublicId));
    }

    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string insertSql =
      """
      INSERT INTO administration.user_roles (user_id, role_name)
      VALUES (@UserId, @RoleName)
      ON CONFLICT (user_id, role_name) DO NOTHING
      """;

    await connection.ExecuteAsync(
      new CommandDefinition(
        insertSql,
        new { user.UserId, command.RoleName },
        cancellationToken: cancellationToken));

    const string permSql =
      $"""
      SELECT permission_code AS {nameof(PermissionRow.PermissionCode)}
      FROM administration.role_permissions
      WHERE role_name = @RoleName
      """;

    var permissionCodes = (await connection.QueryAsync<PermissionRow>(
      new CommandDefinition(
        permSql,
        new { command.RoleName },
        cancellationToken: cancellationToken)))
      .Select(p => p.PermissionCode)
      .ToList();

    await _eventBus.PublishAsync(
      new UserRoleAddedIntegrationEvent(
        id: Guid.NewGuid(),
        occurredOnUtc: _dateTimeProvider.UtcNow,
        userId: user.UserId,
        roleName: command.RoleName,
        permissionCodesToAdd: permissionCodes),
      cancellationToken);

    return Result.Success(true);
  }

  private sealed record PermissionRow(string PermissionCode);
}
