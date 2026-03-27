namespace ScreenDrafts.Modules.Reporting.Domain.Movies;

[Flags]
public enum MoviePositionHonorific
{
  None = 0,

  /// <summary>
  /// Appears in the No. 1 spot on 2 or more draft boards.
  /// </summary>
  UnifiedNumber1 = 1 << 0,

  /// <summary>
  /// Appeared in positions 1, 2, 3 and 4 across canonical drafts.
  /// </summary>
  TheCycle = 1 << 1,
}
