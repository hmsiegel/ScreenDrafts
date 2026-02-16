namespace ScreenDrafts.Modules.Drafts.Features.Drafts.CreateDraft;

public sealed record CreateDraftCommand : ICommand<string>
{
  public required string Title { get; init; }
  public required int DraftType { get; init; }
  public required Guid SeriesId { get; init; }
  public int MinPosition { get; init; }
  public int MaxPosition { get; init; }
  public bool AutoCreateFirstPart { get; init; } = true;
}


