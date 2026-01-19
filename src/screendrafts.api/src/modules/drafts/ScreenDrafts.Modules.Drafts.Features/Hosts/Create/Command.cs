namespace ScreenDrafts.Modules.Drafts.Features.Hosts.Create;

internal sealed record Command : Common.Features.Abstractions.Messaging.ICommand<string>
{
  public required string PersonPublicId { get; init; }
}
