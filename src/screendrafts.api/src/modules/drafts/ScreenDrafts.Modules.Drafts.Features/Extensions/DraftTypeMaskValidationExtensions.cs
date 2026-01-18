namespace ScreenDrafts.Modules.Drafts.Features.Extensions;

internal static class DraftTypeMaskValidationExtensions
{
  public static bool IsValidKnownMask(this DraftTypeMask mask)
  {
    return (mask & ~DraftTypeMask.All) == 0;
  }
}
