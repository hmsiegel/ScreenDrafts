namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Enums;

/// <summary>
/// Where a draft or part was released.
/// </summary>
/// <param name="name">The name.</param>
/// <param name="value">The value.</param>
public sealed class ReleaseChannel(string name, int value)
  : SmartEnum<ReleaseChannel>(name, value)
{
  public static readonly ReleaseChannel MainFeed = new(nameof(MainFeed), 0);
  public static readonly ReleaseChannel Patreon = new(nameof(Patreon), 1);
}
