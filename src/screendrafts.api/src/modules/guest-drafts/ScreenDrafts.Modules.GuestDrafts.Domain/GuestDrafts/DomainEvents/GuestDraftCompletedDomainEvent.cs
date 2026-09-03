namespace ScreenDrafts.Modules.GuestDrafts.Domain.GuestDrafts.DomainEvents;

/// <summary>
/// Raised when Complete() succeeds. Deliberately minimal compared to canonical's
/// PartCompleted (no honorifics/predictions/standings -- those are canonical-only
/// systems, out of scope per the GuestDrafts product summary). TotalPicks and
/// VetoCount are cheap to compute at raise time from state Complete() already
/// walked; richer summary detail is a frontend refetch away if needed later.
/// </summary>
public sealed class GuestDraftCompletedDomainEvent(
  Guid guestDraftId,
  string guestDraftPublicId,
  int totalPicks,
  int vetoCount
) : DomainEvent
{
  public Guid GuestDraftId { get; init; } = guestDraftId;
  public string GuestDraftPublicId { get; init; } = guestDraftPublicId;
  public int TotalPicks { get; init; } = totalPicks;
  public int VetoCount { get; init; } = vetoCount;
}
