namespace ScreenDrafts.Modules.Users.Features.Admin.GetPermissions;

internal sealed class QueryHandler(IDbConnectionFactory dbConnectionFactory) : IQueryHandler<Query, List<string>>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;
  public async Task<Result<List<string>>> Handle(Query query, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);
    const string sql =
        $@"
        SELECT DISTINCT 
          p.code AS {nameof(Permission.Code)}
        FROM users.permissions p
        ";
    var permissions = (await connection.QueryAsync<Permission>(sql)).Select(p => p.Code).ToList();
    return permissions;
  }
}
