namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.SetStatus;

internal sealed record SetGuestDraftStatusCommand : ICommand<SetGuestDraftStatusResponse>
{
  public required string GuestDraftPublicId { get; init; }
  public required string CallerUserPublicId { get; init; }
  public GuestDraftStatusAction Action { get; init; }
}
