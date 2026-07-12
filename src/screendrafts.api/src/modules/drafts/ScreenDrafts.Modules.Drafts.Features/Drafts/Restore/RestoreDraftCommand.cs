namespace ScreenDrafts.Modules.Drafts.Features.Drafts.Restore;

internal sealed record RestoreDraftCommand : ICommand
{
  public required string PublicId { get; init; }
}
