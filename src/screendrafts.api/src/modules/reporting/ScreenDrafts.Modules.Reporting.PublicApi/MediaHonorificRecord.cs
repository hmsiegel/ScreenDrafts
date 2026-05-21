namespace ScreenDrafts.Modules.Reporting.PublicApi;

public sealed record MediaHonorificRecord
{
  /// <summary>
  /// Appearance-count honorific.
  /// 0=None, 1=MarqueeOfFame, 2=HatTrick, 3=GrandSlam, 4=HighFive
  /// </summary>
  public required int AppearanceHonorificValue { get; init; }
  public required string AppearanceHonorificName { get; init; }

  /// <summary>
  /// Position honorific bitmask stored in movie_honorifics.position_honorific.
  /// 0=None, 1=UnifiedNo1, 2=TheCycle
  /// Values are not mutually exclusive — a title can hold both.
  /// </summary>
  public required int PositionHonorificValue { get; init; }

  public required int AppearanceCount { get; init; }

  // Derived convenience properties
  public bool IsUnifiedNo1 => (PositionHonorificValue & 1) != 0;
  public bool IsTheCycle => (PositionHonorificValue & 2) != 0;
}
