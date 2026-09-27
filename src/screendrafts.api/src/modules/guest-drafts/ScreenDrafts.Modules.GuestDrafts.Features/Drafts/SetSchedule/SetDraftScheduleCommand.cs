namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.SetSchedule;

internal sealed record SetDraftScheduleCommand : ICommand
{
  public required string GuestDraftPublicId { get; init; }
  public required string CallerUserPublicId { get; init; }
  public required DateTime ScheduledForUtc { get; init; }
}
