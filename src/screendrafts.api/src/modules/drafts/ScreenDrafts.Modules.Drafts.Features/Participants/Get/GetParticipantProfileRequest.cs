namespace ScreenDrafts.Modules.Drafts.Features.Participants.Get;

internal sealed record GetParticipantProfileRequest
{
  [FromRoute(Name = "personPublicId")]
  public string PersonPublicId { get; init; } = default!;
}
