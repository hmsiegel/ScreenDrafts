using ScreenDrafts.Modules.GuestDrafts.Features.Drafts.SetStatus;

namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.SetStatus;

internal sealed record SetGuestDraftStatusRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;

  public DraftStatusAction Action { get; init; }
}
