namespace ScreenDrafts.Modules.Drafts.Infrastructure.Data;

internal sealed class DbConnectionFactory(NpgsqlDataSource dataSource) : IDbConnectionFactory
{
  private readonly NpgsqlDataSource _dataSource = dataSource;

  public async ValueTask<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
  {
    return await _dataSource.OpenConnectionAsync(cancellationToken);
  }
}
