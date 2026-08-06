namespace ScreenDrafts.Modules.Movies.Infrastructure.Inbox;

internal sealed class InboxOptions
{
  public int IntervalInSeconds { get; init; }
  public int? IntervalInMilliseconds { get; init; }
  public int BatchSize { get; init; }
}
