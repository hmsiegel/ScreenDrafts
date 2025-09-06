namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Enums;

public sealed class ContinuityDateRule(string name, int value)
  : SmartEnum<ContinuityDateRule>(name, value)
{
  /// <summary>
  /// Use earliest release date across all channels (default)
  /// </summary>
  public static readonly ContinuityDateRule AnyChannelFirstRelease =
    new(nameof(AnyChannelFirstRelease), 0);

  /// <summary>
  /// Order by main feed release date only
  /// </summary>
  public static readonly ContinuityDateRule MainFeedOnly =
    new(nameof(MainFeedOnly), 1);
}
