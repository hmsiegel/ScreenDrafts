namespace ScreenDrafts.Modules.Drafts.Features.Hosts.Get;

internal sealed record GetHostRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;
}

