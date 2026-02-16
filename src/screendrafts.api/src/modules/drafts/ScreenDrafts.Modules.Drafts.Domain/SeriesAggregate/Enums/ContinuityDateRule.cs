namespace ScreenDrafts.Modules.Drafts.Domain.SeriesAggregate.Enums;

/// <summary>
/// Defines how to determine the continuity scope for a draft.
/// </summary>
/// <param name="name">The name.</param>
/// <param name="value">The value.</param>

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
