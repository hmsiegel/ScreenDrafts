namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Enums;

public sealed class ContinuityScope(string name, int value)
  : SmartEnum<ContinuityScope>(name, value)
{
  /// <summary>
  /// No rollover beyond a single draft
  /// </summary>
  public static readonly ContinuityScope None = new(nameof(None), 0);

  /// <summary>
  /// Rollover only within the same series
  /// </summary>
  public static readonly ContinuityScope Series = new(nameof(Series), 1);

  /// <summary>
  /// Rollover across all drafts (default)
  /// </summary>
  public static readonly ContinuityScope Global = new(nameof(Global), 2);
}
