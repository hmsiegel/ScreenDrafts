namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Enums;

[Flags]
public enum DraftTypeMask
{
  None = 0,
  Standard = 1 << DraftTypeValue.Standard,
  MiniMega = 1 << DraftTypeValue.MiniMega,
  Mega = 1 << DraftTypeValue.Mega,
  Super = 1 << DraftTypeValue.Super,
  MiniSuper = 1 << DraftTypeValue.MiniSuper,
  All = Standard | MiniMega | Mega | Super | MiniSuper
}

internal static class DraftTypeValue
{
  public const int Standard = 0;
  public const int MiniMega = 1;
  public const int Mega = 2;
  public const int Super = 3;
  public const int MiniSuper = 4;
}
