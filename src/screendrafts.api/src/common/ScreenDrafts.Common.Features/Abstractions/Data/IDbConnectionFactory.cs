namespace ScreenDrafts.Common.Features.Abstractions.Data;

public interface IDbConnectionFactory
{
  ValueTask<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default);
}
