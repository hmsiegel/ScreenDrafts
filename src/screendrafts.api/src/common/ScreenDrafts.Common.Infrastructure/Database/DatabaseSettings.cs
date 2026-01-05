namespace ScreenDrafts.Common.Infrastructure.Database;

public sealed record DatabaseSettings
{
  public string ConnectionString { get; set; } = default!;
}
