namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.UndoVeto;

// ── Request ───────────────────────────────────────────────────────────────────

internal sealed record UndoVetoRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartId { get; init; } = default!;

  [FromRoute(Name = "playOrder")]
  public int PlayOrder { get; init; }
}
