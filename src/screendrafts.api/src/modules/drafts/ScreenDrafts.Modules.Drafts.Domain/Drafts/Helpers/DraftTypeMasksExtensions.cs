namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Helpers;
public static class DraftTypeMasksExtensions
{
  public static int ToMask(params DraftType[] draftTypes) =>
    draftTypes.Aggregate(0, (mask, t) => mask | (1 << t.Value));

  public static bool Allows(this int mask, DraftType draftType)
  {
    ArgumentNullException.ThrowIfNull(draftType);
    return (mask & (1 << draftType.Value)) != 0;
  }
}

