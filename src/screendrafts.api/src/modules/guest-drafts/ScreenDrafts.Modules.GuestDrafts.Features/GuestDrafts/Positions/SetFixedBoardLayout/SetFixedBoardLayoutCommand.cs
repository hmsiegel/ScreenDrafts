namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Positions.SetFixedBoardLayout;

internal sealed record SetFixedBoardLayoutCommand : ICommand
{
  public required string GuestDraftPublicId { get; init; }
  public required string CallerUserPublicId { get; init; }
}
