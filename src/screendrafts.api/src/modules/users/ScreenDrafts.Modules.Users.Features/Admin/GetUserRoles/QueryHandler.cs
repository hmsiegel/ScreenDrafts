namespace ScreenDrafts.Modules.Users.Features.Admin.GetUserRoles;

internal sealed class QueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<Query, IReadOnlyCollection<string>>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<IReadOnlyCollection<string>>> Handle(Query request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql =
      $@"
          SELECT DISTINCT 
            ur.role_name AS {nameof(Role.Name)}
          FROM users.user_roles ur
          WHERE ur.user_id = @UserId
          ";

    var roles = (await connection.QueryAsync<Role>(sql, new { request.UserId })).ToList();

    if (roles.Count == 0)
    {
      return Result.Failure<IReadOnlyCollection<string>>(UserErrors.NotFound(request.UserId));
    }

    // Map Role objects to their Name property and return as IReadOnlyCollection<string>
    var roleNames = roles.Select(role => role.Name).ToList().AsReadOnly();

    return Result.Success<IReadOnlyCollection<string>>(roleNames);
  }
}
