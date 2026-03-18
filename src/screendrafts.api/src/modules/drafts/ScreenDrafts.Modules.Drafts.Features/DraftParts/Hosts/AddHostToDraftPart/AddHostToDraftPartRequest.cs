namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Hosts.AddHostToDraftPart;

internal sealed record AddHostToDraftPartRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartId { get; init; } = default!;
  public string HostPublicId { get; init; } = default!;
  public int HostRole { get; init; } = default!;
}
