namespace ScreenDrafts.Modules.Drafts.Features.Categories.Create;

internal sealed record Command : Common.Features.Abstractions.Messaging.ICommand<string>
{
  public required string Name { get; init; }
  public required string Description { get; init; }
}


