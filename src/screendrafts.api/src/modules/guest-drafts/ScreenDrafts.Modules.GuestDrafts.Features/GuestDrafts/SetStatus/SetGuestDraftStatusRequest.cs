namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.SetStatus;

internal sealed record SetGuestDraftStatusRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;

  public GuestDraftStatusAction Action { get; init; }
}
