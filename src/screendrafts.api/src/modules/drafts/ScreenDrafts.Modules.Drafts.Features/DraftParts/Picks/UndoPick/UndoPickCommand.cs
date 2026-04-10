namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.UndoPick;

internal sealed record UndoPickCommand : ICommand
{
  public required string DraftPartPublicId { get; init; }
  public int PlayOrder { get; init; }
  public string? SubDraftPublicId { get; init; }
}
