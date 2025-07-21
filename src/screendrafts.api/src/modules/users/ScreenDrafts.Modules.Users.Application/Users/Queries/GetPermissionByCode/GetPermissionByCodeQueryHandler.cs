namespace ScreenDrafts.Modules.Users.Application.Users.Queries.GetPermissionByCode;

internal sealed class GetPermissionByCodeQueryHandler(IDbConnectionFactory dbConnectionFactory) : IQueryHandler<GetPermissionByCodeQuery, PermissionResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;
  public async Task<Result<PermissionResponse>> Handle(GetPermissionByCodeQuery query, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);
    const string sql =
      $@"
      SELECT DISTINCT 
        p.code AS {nameof(PermissionResponse.Code)}
      FROM users.permissions p
      WHERE p.code = @Code
      ";
    var permission = await connection.QueryFirstOrDefaultAsync<PermissionResponse>(sql, query);

    if (permission is null)
    {
      return Result.Failure<PermissionResponse>(UserErrors.PermissionNotFound(query.Code));
    }

    return permission;
  }
}
