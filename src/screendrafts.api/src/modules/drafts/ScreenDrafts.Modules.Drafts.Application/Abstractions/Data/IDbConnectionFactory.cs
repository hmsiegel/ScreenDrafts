namespace ScreenDrafts.Modules.Drafts.Application.Abstractions.Data;

public interface IDbConnectionFactory
{
  ValueTask<DbConnection> OpenConnectionAsync();
}
