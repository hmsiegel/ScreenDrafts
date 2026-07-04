namespace ScreenDrafts.Modules.GuestDrafts.Infrastructure.Outbox;

internal sealed class OutboxOptions
{
  public int IntervalInSeconds { get; init; }

  public int BatchSize { get; init; }
}
