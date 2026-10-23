namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Enums;

/// <summary>
/// Where a release link is in the lookup process.
/// </summary>
/// <param name="name">The name.</param>
/// <param name="value">The value.</param>
public sealed class ReleaseLinkStatus(string name, int value)
  : SmartEnum<ReleaseLinkStatus>(name, value)
{
  public static readonly ReleaseLinkStatus Pending = new(nameof(Pending), 0);
  public static readonly ReleaseLinkStatus Linked = new(nameof(Linked), 1);
  public static readonly ReleaseLinkStatus NeedsReview = new(nameof(NeedsReview), 2);
  public static readonly ReleaseLinkStatus NotFound = new(nameof(NotFound), 3);
}
