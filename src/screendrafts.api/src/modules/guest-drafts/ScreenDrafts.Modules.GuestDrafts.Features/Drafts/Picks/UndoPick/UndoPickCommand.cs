namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Picks.UndoPick;

internal sealed record UndoPickCommand : ICommand
{
  public required string GuestDraftPublicId { get; init; }
  public required int PlayOrder { get; init; }
  public required string CallerUserPublicId { get; init; }
}
