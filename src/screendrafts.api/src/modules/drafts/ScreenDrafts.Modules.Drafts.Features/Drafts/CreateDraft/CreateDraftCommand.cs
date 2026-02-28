namespace ScreenDrafts.Modules.Drafts.Features.Drafts.CreateDraft;

internal sealed record CreateDraftCommand : ICommand<string>
{
  public required string Title { get; init; }
  public required int DraftType { get; init; }
  public required Guid SeriesId { get; init; }
}


