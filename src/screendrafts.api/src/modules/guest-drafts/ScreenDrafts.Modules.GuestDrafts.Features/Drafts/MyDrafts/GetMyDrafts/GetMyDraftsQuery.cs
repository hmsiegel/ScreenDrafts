// ── Query ─────────────────────────────────────────────────────────────────────
namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.MyDrafts.GetMyDrafts;

internal sealed record GetMyDraftsQuery : IQuery<GetMyGuestDraftsResponse>
{
  public required string CallerUserPublicId { get; init; }
}
