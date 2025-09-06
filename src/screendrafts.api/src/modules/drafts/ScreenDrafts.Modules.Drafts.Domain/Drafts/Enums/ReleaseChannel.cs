namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Enums;

public sealed class ReleaseChannel(string name, int value)
  : SmartEnum<ReleaseChannel>(name, value)
{
  public static readonly ReleaseChannel MainFeed = new(nameof(MainFeed), 0);
  public static readonly ReleaseChannel Patreon = new(nameof(Patreon), 1);
}
