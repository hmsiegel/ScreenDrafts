namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Picks.UndoVeto;

internal sealed record UndoVetoCommand : ICommand
{
  public required string GuestDraftPublicId { get; init; }
  public required int PlayOrder { get; init; }
  public required string CallerUserPublicId { get; init; }
}
