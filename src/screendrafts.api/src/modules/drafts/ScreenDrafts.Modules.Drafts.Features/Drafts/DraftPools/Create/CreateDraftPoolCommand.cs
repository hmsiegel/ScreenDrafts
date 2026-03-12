namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftPools.Create;

internal sealed record CreateDraftPoolCommand : ICommand
{
  public required string PublicId { get; init; }
}
