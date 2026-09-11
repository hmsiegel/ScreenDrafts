namespace ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.DomainEvents;

/// <summary>
/// Raised when Complete() succeeds. Deliberately minimal compared to canonical's
/// PartCompleted (no honorifics/predictions/standings -- those are canonical-only
/// systems, out of scope per the GuestDrafts product summary). TotalPicks and
/// VetoCount are cheap to compute at raise time from state Complete() already
/// walked; richer summary detail is a frontend refetch away if needed later.
/// </summary>
public sealed class DraftCompletedDomainEvent(
  Guid draftId,
  string draftPublicId,
  int totalPicks,
  int vetoCount
) : DomainEvent
{
  public Guid DraftId { get; init; } = draftId;
  public string DraftPublicId { get; init; } = draftPublicId;
  public int TotalPicks { get; init; } = totalPicks;
  public int VetoCount { get; init; } = vetoCount;
}
