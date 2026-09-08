namespace ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Helpers;

/// <summary>
/// Fixed board layouts for the guest draft types that don't allow custom positions --
/// mirrors positions-editor.tsx's getDefaultPositions/isFixedPositionType exactly for
/// Standard and MiniSuper (the two fixed types confirmed for guest drafts; the
/// frontend's SpeedDraft template isn't relevant here since SpeedDraft isn't a
/// GuestDraftType). MiniMega, Super, and Mega are custom-defined by the draft owner
/// via GuestDraft.SetCustomPositions.
/// </summary>
public static class GameBoardTemplates
{
  internal sealed record PositionTemplate(
    string Name,
    IReadOnlyList<int> Picks,
    bool HasBonusVeto = false,
    bool HasBonusVetoOverride = false,
    bool HasBonusFungibleToken = false
  );

  public static bool IsFixed(DraftType type) =>
    type == DraftType.Standard || type == DraftType.MiniSuper;

  internal static IReadOnlyList<PositionTemplate>? GetFixedTemplate(DraftType type)
  {
    if (type == DraftType.Standard)
    {
      return [new PositionTemplate("A", [7, 6, 4, 2]), new PositionTemplate("B", [5, 3, 1])];
    }

    if (type == DraftType.MiniSuper)
    {
      return [new PositionTemplate("A", [5, 3, 1]), new PositionTemplate("B", [4, 2])];
    }

    return null;
  }
}
