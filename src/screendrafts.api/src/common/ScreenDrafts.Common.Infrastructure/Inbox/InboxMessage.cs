namespace ScreenDrafts.Common.Infrastructure.Inbox;

public sealed class InboxMessage
{
  public Guid Id { get; init; }

  public string Type { get; init; } = default!;

  public string Content { get; init; } = default!;

  public DateTime OccurredOnUtc { get; init; }

  public DateTime? ProcessedOnUtc { get; init; }

  public string? Error { get; init; }
}
