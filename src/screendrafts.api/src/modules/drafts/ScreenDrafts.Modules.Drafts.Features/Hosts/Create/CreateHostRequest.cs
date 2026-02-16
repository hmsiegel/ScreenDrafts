namespace ScreenDrafts.Modules.Drafts.Features.Hosts.Create;

internal sealed record CreateHostRequest
{
  public required string PersonPublicId { get; init; }
}

