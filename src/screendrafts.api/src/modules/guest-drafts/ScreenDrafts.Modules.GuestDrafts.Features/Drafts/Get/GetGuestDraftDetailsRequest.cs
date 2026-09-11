namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Get;

internal sealed record GetGuestDraftDetailsRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;
}
