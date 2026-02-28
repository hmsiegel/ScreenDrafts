namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.UndoPick;

internal sealed record UndoPickCommand : ICommand
{
  public required string DraftPartPublicId { get; init; }
  public int PlayOrder { get; init; }
}
