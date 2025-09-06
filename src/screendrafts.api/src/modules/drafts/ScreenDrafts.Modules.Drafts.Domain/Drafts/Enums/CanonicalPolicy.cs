namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Enums;

public sealed class CanonicalPolicy(string name, int value)
  : SmartEnum<CanonicalPolicy>(name, value)
{
  /// <summary>
  /// Regular, main feed eligible series
  /// </summary>
  public static readonly CanonicalPolicy Always = new(nameof(Always), 0);

  /// <summary>
  /// Series that are never canonical, i.e. Legends, Franchise mini-Super,  Best Picture
  /// </summary>
  public static readonly CanonicalPolicy Never = new(nameof(Never), 1);

  /// <summary>
  /// Patreon series that become canonical only when a part (or whole) hits the main feed.
  /// </summary>
  public static readonly CanonicalPolicy OnMainFeed = new(nameof(OnMainFeed), 2);
}
