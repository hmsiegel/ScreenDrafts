namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.MyDrafts.GetMyDrafts;

internal sealed record MyGuestDraftSummary
{
  public required string PublicId { get; init; }
  public required string Title { get; init; }
  public required string Type { get; init; }
  public required string Status { get; init; }
  public DateTime? ScheduledForUtc { get; init; }
  public bool IsOwner { get; init; }
}
