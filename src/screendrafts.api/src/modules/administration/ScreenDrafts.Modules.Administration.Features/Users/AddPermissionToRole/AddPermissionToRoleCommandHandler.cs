namespace ScreenDrafts.Modules.Administration.Features.Users.AddPermissionToRole;

internal sealed class AddPermissionToRoleCommandHandler(
  IEventBus eventBus,
  IDateTimeProvider dateTimeProvider,
  IDbConnectionFactory dbConnectionFactory)
  : ICommandHandler<AddPermissionToRoleCommand, bool>
{
  private readonly IEventBus _eventBus = eventBus;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<bool>> Handle(
    AddPermissionToRoleCommand request,
    CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string permissionExistsSql =
      """
      SELECT COUNT(1)
      FROM administration.permissions p
      WHERE p.code = @PermissionCode
      """;

    var permissionExists = await connection.ExecuteScalarAsync<int>(
      new CommandDefinition(
        permissionExistsSql,
        new { request.PermissionCode },
        cancellationToken: cancellationToken)) > 0;

    if (!permissionExists)
    {
      return Result.Failure<bool>(AdministrationErrors.PermissionNotFound(request.PermissionCode));
    }

    const string alreadyAssignedSql =
      """
      SELECT COUNT(1)
      FROM administration.role_permissions rp
      WHERE rp.role_name = @RoleName AND rp.permission_code = @PermissionCode
      """;

    var alreadyAssigned = await connection.ExecuteScalarAsync<int>(
      new CommandDefinition(
        alreadyAssignedSql,
        new { request.PermissionCode, request.RoleName },
        cancellationToken: cancellationToken)) > 0;

    if (alreadyAssigned)
    {
      return Result.Failure<bool>(AdministrationErrors.PermissionAlreadyAssigned(
        request.RoleName,
        request.PermissionCode));
    }

    const string insertSql =
      """
      INSERT INTO administration.role_permissions (role_name, permission_code)
      VALUES (@RoleName, @PermissionCode)
      """;

    await connection.ExecuteAsync(
      new CommandDefinition(
        insertSql,
        new { request.RoleName, request.PermissionCode },
        cancellationToken: cancellationToken));

    const string usersSql =
      $"""
      SELECT ur.user_id AS {nameof(UserRow.UserId)}
      FROM administration.user_roles ur
      WHERE ur.role_name = @RoleName
      """;

    var affectedUsers = (await connection.QueryAsync<UserRow>(
      new CommandDefinition(
        usersSql,
        new { request.RoleName },
        cancellationToken: cancellationToken)))
     .Select(r => r.UserId)
     .ToList();

    await _eventBus.PublishAsync(
      new PermissionAddedToRoleIntegrationEvent(
        id: Guid.NewGuid(),
        occurredOnUtc: _dateTimeProvider.UtcNow,
        roleName: request.RoleName,
        permissionCode: request.PermissionCode,
        affectedUserIds: affectedUsers),
      cancellationToken: cancellationToken);

    return Result.Success(true);
  }

  private sealed record UserRow(Guid UserId);
}
