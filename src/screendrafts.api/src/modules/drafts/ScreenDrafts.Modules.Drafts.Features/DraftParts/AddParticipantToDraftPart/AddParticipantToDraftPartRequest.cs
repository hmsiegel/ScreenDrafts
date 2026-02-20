namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.AddParticipantToDraftPart;

internal sealed record AddParticipantToDraftPartRequest(
  [FromRoute(Name = "draftPartId")] Guid DraftPartId,
  [FromBody] string? ParticipantPublicId,
  [FromBody] int ParticipantKind);

