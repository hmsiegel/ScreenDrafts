namespace ScreenDrafts.Modules.Users.Features.Admin.AddPermission;

internal sealed class AddPermissionCommandHandler(IDbConnectionFactory dbConnectionFactory)
  : ICommandHandler<AddPermissionCommand, bool>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<bool>> Handle(AddPermissionCommand request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string getSql =
      $"""
        SELECT DISTINCT code
        FROM users.permissions
      """;

    var permissions = (await connection.QueryAsync<Permission>(getSql)).AsList();

    if (permissions.Any(p => p.Code == request.Code))
    {
      return Result.Failure<bool>(UserErrors.PermissionAlreadyExists(request.Code));
    }

    const string insertSql =
      $"""
        INSERT INTO users.permissions (code)
        VALUES (@Code)
      """;

    await connection.ExecuteAsync(insertSql, request);

    return Result.Success(true);
  }
}
