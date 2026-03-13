namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Hosts.AddHostToDraftPart;

internal sealed record AddHostToDraftPartRequest(
  [FromRoute(Name = "draftPartId")] Guid DraftPartId,
  [FromBody] string HostPublicId,
  [FromBody] int HostRole);
