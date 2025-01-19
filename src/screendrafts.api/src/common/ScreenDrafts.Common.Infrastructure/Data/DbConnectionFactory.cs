namespace ScreenDrafts.Common.Infrastructure.Data;

internal sealed class DbConnectionFactory(NpgsqlDataSource dataSource) : IDbConnectionFactory
{
  public async ValueTask<DbConnection> OpenConnectionAsync()
  {
    return await dataSource.OpenConnectionAsync();
  }
}
