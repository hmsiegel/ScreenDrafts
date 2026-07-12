namespace ScreenDrafts.Modules.Drafts.Features.Drafts.Delete;

internal sealed record DeleteDraftCommand : ICommand
{
  public required string PublicId { get; init; }
}
