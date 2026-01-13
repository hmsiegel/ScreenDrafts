namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.RemoveHost;

public sealed record Request(
  [FromRoute(Name="draftPartId")]Guid DraftPartId,
  [FromRoute(Name="hostId")]Guid HostId);
