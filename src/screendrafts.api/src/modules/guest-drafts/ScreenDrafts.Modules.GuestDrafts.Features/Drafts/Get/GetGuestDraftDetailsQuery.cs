namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Get;

internal sealed record GetGuestDraftDetailsQuery : IQuery<GuestDraftDetailResponse>
{
  public required string GuestDraftPublicId { get; init; }
  public required string CallerUserPublicId { get; init; }
}
