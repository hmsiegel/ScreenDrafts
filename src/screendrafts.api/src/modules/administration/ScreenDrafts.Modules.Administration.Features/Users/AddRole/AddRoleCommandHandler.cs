namespace ScreenDrafts.Modules.Administration.Features.Users.AddRole;

internal sealed class AddRoleCommandHandler(IDbConnectionFactory connectionFactory)
  : ICommandHandler<AddRoleCommand, bool>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

  public async Task<Result<bool>> Handle(AddRoleCommand request, CancellationToken cancellationToken)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    var existingRole = await connection.QueryFirstOrDefaultAsync<int>(
      new CommandDefinition(
        "SELECT COUNT (1) FROM administration.roles WHERE name = @Name",
        new { request.Name },
        cancellationToken: cancellationToken)) > 0;

    if (existingRole)
    {
      return Result.Failure<bool>(AdministrationErrors.RoleAlreadyExists(request.Name));
    }

    await connection.ExecuteAsync(
      new CommandDefinition(
        "INSERT INTO administration.roles (name) VALUES (@Name)",
        new { request.Name },
        cancellationToken: cancellationToken));

    return Result.Success(true);
  }
}
