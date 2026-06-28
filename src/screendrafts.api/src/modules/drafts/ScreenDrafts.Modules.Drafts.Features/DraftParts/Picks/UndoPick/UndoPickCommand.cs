namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.UndoPick;

internal sealed record UndoPickCommand : ICommand
{
  public required string DraftPartId { get; init; }
  public int PlayOrder { get; init; }
  public string? SubDraftPublicId { get; init; }
}
