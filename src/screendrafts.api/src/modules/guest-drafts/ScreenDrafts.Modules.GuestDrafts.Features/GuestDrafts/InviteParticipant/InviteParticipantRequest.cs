namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.InviteParticipant;

internal sealed record InviteParticipantRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;
  public required string UserPublicId { get; init; }
}
