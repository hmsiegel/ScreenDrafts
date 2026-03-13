namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Hosts.RemoveHost;

public sealed record RemoveHostDraftPartRequest(
  [FromRoute(Name="draftPartId")]Guid DraftPartId,
  [FromRoute(Name="hostId")]Guid HostId);

