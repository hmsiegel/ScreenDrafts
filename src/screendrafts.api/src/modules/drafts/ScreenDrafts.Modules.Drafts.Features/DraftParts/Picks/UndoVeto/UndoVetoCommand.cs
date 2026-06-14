namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.UndoVeto;

// ── Command ───────────────────────────────────────────────────────────────────

internal sealed record UndoVetoCommand : ICommand
{
  public required string DraftPartId { get; init; }
  public required int PlayOrder { get; init; }
}
