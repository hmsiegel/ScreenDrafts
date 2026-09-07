namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Positions.AssignParticipantToPosition;

internal sealed record AssignParticipantToPositionCommand : ICommand
{
  public required string GuestDraftPublicId { get; init; }
  public required string PositionPublicId { get; init; }
  public required string CallerUserPublicId { get; init; }

  // Team support deferred -- only GuestDrafter public ids are accepted for
  // now, same as AddParticipant.
  public required string GuestDrafterPublicId { get; init; }
}
