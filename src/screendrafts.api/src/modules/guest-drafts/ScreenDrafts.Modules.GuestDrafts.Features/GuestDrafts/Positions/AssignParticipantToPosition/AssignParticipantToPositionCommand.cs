namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Positions.AssignParticipantToPosition;

internal sealed record AssignParticipantToPositionCommand : ICommand
{
  public required string GuestDraftPublicId { get; init; }
  public required string PositionPublicId { get; init; }
  public required string CallerUserPublicId { get; init; }
  public required string ParticipantPublicId { get; init; }
}
