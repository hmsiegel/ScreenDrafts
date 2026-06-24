namespace ScreenDrafts.Modules.Drafts.Features.Drafts.CreateDraft;

internal sealed record CreateDraftCommand : ICommand<string>
{
  public required string Title { get; init; }
  public required int DraftType { get; init; }
  public required string SeriesId { get; init; }

  public IReadOnlyList<CreateDraftPartInput> Parts { get; init; } = [];
  public IReadOnlyList<CreateDraftHostInput> Hosts { get; init; } = [];
  public IReadOnlyList<string> DrafterIds { get; init; } = [];
  public IReadOnlyList<string> TeamIds { get; init; } = [];
  public IReadOnlyList<string> CategoryIds { get; init; } = [];
  public string? CampaignId { get; init; }
}
