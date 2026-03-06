namespace ScreenDrafts.Modules.Drafts.Features.Hosts.Get;

internal sealed record GetHostRequest
{
  [FromRoute(Name = "publicId")]
  public required string PublicId { get; init; }
}

