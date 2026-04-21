namespace ScreenDrafts.Modules.Administration.Features.Users.AddPermission;

internal sealed class AddPermissionCommandHandler(IDbConnectionFactory connectionFactory)
  : ICommandHandler<AddPermissionCommand>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

  public async Task<Result> Handle(AddPermissionCommand request, CancellationToken cancellationToken)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    const string existsSql =
      """
      SELECT COUNT(1)
      FROM administration.permissions
      WHERE code = @Code
      """;

    var exists = await connection.ExecuteScalarAsync<int>(
      new CommandDefinition(
        existsSql,
        new { request.Code },
        cancellationToken: cancellationToken)) > 0;

    if (exists)
    {
      return Result.Failure(AdministrationErrors.PermissionAlreadyExists(request.Code));
    }

    await connection.ExecuteAsync(
      new CommandDefinition(
        """
        INSERT INTO administration.permissions (code)
        VALUES (@Code)
        """,
        new { request.Code },
        cancellationToken: cancellationToken));

    return Result.Success();
  }
}
