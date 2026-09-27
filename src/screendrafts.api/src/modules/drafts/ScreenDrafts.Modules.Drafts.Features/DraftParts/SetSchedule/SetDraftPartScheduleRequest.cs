namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SetSchedule;

// ── Request ───────────────────────────────────────────────────────────────────
internal sealed class SetDraftPartScheduleRequest
{
  public string DraftPartId { get; init; } = default!;
  public required DateTime ScheduledForUtc { get; init; }
}
