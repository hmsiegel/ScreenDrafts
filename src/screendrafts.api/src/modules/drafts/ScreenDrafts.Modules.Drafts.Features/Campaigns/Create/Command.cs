namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Create;

internal sealed record Command : Common.Features.Abstractions.Messaging.ICommand<string>
{
  public required string Name { get; init; }
  public required string Slug { get; init; }
}


