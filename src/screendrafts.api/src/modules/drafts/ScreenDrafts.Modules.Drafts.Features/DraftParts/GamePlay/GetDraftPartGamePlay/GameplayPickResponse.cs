namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.GamePlay.GetDraftPartGamePlay;

internal sealed record GameplayPickResponse
{
  public int PlayOrder { get; init; }
  public int BoardPosition { get; init; }
  public string MovieTitle { get; init; } = default!;
  public string? MovieYear { get; init; }
  public int TmdbId { get; init; }
  public Guid PlayedById { get; init; }
  public int PlayedByKind { get; init; }
  public string PlayedByName { get; init; } = default!;
  public bool WasVetoed { get; init; }
  public bool WasVetoOverridden { get; init; }
  public bool WasCommissionerOverride { get; init; }
}
