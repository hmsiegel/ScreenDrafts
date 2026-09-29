namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.SetSchedule;

// ── Request ───────────────────────────────────────────────────────────────────
internal sealed record SetDraftScheduleRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;

  public required DateTime ScheduledForUtc { get; init; }
}
