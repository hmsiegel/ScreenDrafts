// ── Command ──────────────────────────────────────────────────────────────────

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SetSchedule;

internal sealed record SetDraftPartScheduleCommand : ICommand
{
  public required string DraftPartId { get; init; }
  public required DateTime ScheduledForUtc { get; init; }
}
