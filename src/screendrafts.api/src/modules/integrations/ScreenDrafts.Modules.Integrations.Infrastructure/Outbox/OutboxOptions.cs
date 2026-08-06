namespace ScreenDrafts.Modules.Integrations.Infrastructure.Outbox;

internal sealed class OutboxOptions
{
  public int IntervalInSeconds { get; init; }
  public int? IntervalInMilliseconds { get; init; }
  public int BatchSize { get; init; }
}
