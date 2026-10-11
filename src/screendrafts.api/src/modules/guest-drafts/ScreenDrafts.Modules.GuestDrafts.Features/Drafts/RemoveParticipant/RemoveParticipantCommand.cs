namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.RemoveParticipant;

// ── Command ───────────────────────────────────────────────────────────────────
internal sealed record RemoveParticipantCommand : ICommand
{
  public required string GuestDraftPublicId { get; init; }
  public required string CallerUserPublicId { get; init; }
  public required string GuestDrafterPublicId { get; init; }
}
