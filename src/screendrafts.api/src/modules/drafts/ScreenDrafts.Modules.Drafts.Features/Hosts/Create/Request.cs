namespace ScreenDrafts.Modules.Drafts.Features.Hosts.Create;

internal sealed record Request
{
  public required string PersonPublicId { get; init; }
}
