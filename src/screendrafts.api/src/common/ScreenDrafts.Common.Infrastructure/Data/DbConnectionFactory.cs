namespace ScreenDrafts.Common.Infrastructure.Data;

internal sealed class DbConnectionFactory(NpgsqlDataSource dataSource) : IDbConnectionFactory
{
  public async ValueTask<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
  {
    return await dataSource.OpenConnectionAsync(cancellationToken);
  }
}
