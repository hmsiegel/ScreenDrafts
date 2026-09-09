namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.AddParticipant;

internal sealed record AddParticipantRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;

  public required string GuestDrafterPublicId { get; init; }
}
