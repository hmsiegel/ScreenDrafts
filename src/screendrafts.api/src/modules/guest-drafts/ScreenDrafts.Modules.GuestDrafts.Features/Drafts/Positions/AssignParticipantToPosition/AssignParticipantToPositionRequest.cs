namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Positions.AssignParticipantToPosition;

internal sealed record AssignParticipantToPositionRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;

  [FromRoute(Name = "positionPublicId")]
  public string PositionPublicId { get; init; } = default!;

  public required string GuestDrafterPublicId { get; init; }
}
