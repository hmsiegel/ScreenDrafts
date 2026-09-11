namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.AddParticipant;

internal sealed record AddParticipantCommand : ICommand
{
  public required string GuestDraftPublicId { get; init; }
  public required string CallerUserPublicId { get; init; }

  // Team support deferred -- only GuestDrafter public ids are accepted for
  // now. Add a Kind field alongside this when Teams actually ship.
  public required string GuestDrafterPublicId { get; init; }
}
