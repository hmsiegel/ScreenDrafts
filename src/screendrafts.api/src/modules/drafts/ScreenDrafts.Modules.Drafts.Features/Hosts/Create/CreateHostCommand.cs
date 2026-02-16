namespace ScreenDrafts.Modules.Drafts.Features.Hosts.Create;

internal sealed record CreateHostCommand : ICommand<string>
{
  public required string PersonPublicId { get; init; }
}

