namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.PlayPick;

internal sealed record PlayPickRequest(
  [FromRoute(Name = "draftPartId")] Guid DraftPartId,
  int Position,
  int PlayOrder,
  string? ParticipantPublicId,
  int ParticipantKind,
  Guid MovieId,
  string? MovieVersionName);
