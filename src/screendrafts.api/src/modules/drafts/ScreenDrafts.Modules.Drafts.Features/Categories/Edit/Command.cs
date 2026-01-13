namespace ScreenDrafts.Modules.Drafts.Features.Categories.Edit;

internal sealed record Command : Common.Features.Abstractions.Messaging.ICommand
{
  public string PublicId { get; init; } = string.Empty;
  public string? Name { get; init; }
  public string? Description { get; init; }
}
