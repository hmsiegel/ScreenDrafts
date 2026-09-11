namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.SetStatus;

internal sealed record SetGuestDraftStatusResponse
{
  public string GuestDraftPublicId { get; init; } = default!;
  public string Status { get; init; } = default!;
}
