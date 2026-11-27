namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Participants.SetPhantomParticipant;

internal sealed record SetPhantomParticipantRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartId { get; init; } = default!;

  public string ParticipantPublicId { get; init; } = default!;

  public bool IsPhantom { get; init; }
}
