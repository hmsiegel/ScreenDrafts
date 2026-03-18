namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Hosts.RemoveHost;

public sealed record RemoveHostDraftPartRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartId { get; init; } = default!;

  [FromRoute(Name = "hostId")]
  public string HostId { get; init; } = default!;
}
