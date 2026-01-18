namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Helpers;

public static class DraftTypeMaskExtensions
{
  public static bool Allows(this DraftTypeMask mask, DraftType? draftType)
  {
    if (draftType is null)
    {
      return false;
    }
    return (mask & (DraftTypeMask)(1 << (draftType.Value - 1))) != 0;
  }

  public static bool IsSingleFlag(this DraftTypeMask mask)
  {
    var value = (int)mask;
    return value != 0 && (value & (value - 1)) == 0;
  }
}
