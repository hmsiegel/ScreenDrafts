namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.AddParticipantToDraftPart;

internal sealed record AddParticipantToDraftPartRequest(
  [FromRoute(Name = "draftPartId")] string DraftPartPublicId,
  [FromBody] string? ParticipantPublicId,
  [FromBody] int ParticipantKind);

