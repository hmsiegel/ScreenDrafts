namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.Enums;

public sealed class CarryoverKind(string name, int value) : SmartEnum<CarryoverKind>(name, value)
{
  /// <summary>
  /// No carryover. No points will be carried over to the next round. This is the default option.
  /// </summary>
  public static readonly CarryoverKind None = new(nameof(None), 0);

  /// <summary>
  /// Points carried in from the previous season loss margin.
  /// </summary>
  public static readonly CarryoverKind Handicap = new(nameof(Handicap), 1);

  /// <summary>
  /// Bonus points awarded manually.
  /// </summary>
  public static readonly CarryoverKind Bonus = new(nameof(Bonus), 2);

  /// <summary>
  /// Any other administative carryover.
  /// </summary>
  public static readonly CarryoverKind Manual = new(nameof(Manual), 3);
}
