namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.GamePlay.GetDraftPartGamePlay;

/// <summary>
/// One entry in a pick's full veto history (GameplayPickResponse.VetoHistory). Sequence
/// mirrors Veto.Sequence — 1 for a pick's first veto, 2 for a re-veto after that veto's
/// override was itself overridden, and so on.
/// </summary>
internal sealed record GameplayVetoHistoryEntryResponse
{
  public int Sequence { get; init; }
  public string VetoedByName { get; init; } = default!;
  public bool WasVetoFungible { get; init; }
  public bool IsOverridden { get; init; }
  public string? OverriddenByName { get; init; }
  public bool WasOverrideFungible { get; init; }
}
