namespace ScreenDrafts.Modules.Drafts.Features.Participants.Get;

internal sealed record GetParticipantProfileRequest
{
  [FromRoute(Name = "personPublicId")]
  public string PersonPublicId { get; init; } = default!;

  /// <summary>"patreon" for the Patreon feed; anything else is the main feed.</summary>
  [FromQuery(Name = "channel")]
  public string? Channel { get; init; }
}
