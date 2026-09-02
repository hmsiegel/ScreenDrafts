namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Positions.AssignParticipantToPosition;

internal sealed record AssignParticipantToPositionRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;

  [FromRoute(Name = "positionPublicId")]
  public string PositionPublicId { get; init; } = default!;

  public required string ParticipantPublicId { get; init; }
}
