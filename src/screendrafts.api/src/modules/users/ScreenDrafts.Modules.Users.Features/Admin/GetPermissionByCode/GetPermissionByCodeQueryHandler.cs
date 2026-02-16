namespace ScreenDrafts.Modules.Users.Features.Admin.GetPermissionByCode;

internal sealed class GetPermissionByCodeQueryHandler(IDbConnectionFactory dbConnectionFactory) : IQueryHandler<GetPermissionByCodeQuery, Response>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;
  public async Task<Result<Response>> Handle(GetPermissionByCodeQuery query, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);
    const string sql =
        $@"
        SELECT DISTINCT 
          p.code AS {nameof(Response.Code)}
        FROM users.permissions p
        WHERE p.code = @Code
        ";
    var permission = await connection.QuerySingleOrDefaultAsync<Response>(sql, new { Code = query.Code });
    return permission;
  }
}
