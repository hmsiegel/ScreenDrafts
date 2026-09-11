namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.SetStatus;

internal sealed record SetDraftStatusCommand : ICommand<SetGuestDraftStatusResponse>
{
  public required string GuestDraftPublicId { get; init; }
  public required string CallerUserPublicId { get; init; }
  public DraftStatusAction Action { get; init; }
}
