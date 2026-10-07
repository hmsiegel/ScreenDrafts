namespace ScreenDrafts.Modules.Drafts.Features.Drafts.CreateDraft;

internal sealed record CreateDraftRequest
{
  public required string Title { get; init; }
  public required int DraftType { get; init; }
  public required string SeriesId { get; init; }

  public IReadOnlyList<CreateDraftPartRequest> Parts { get; init; } = [];
  public IReadOnlyList<CreateDraftHostRequest> Hosts { get; init; } = [];
  public IReadOnlyList<string> DrafterIds { get; init; } = [];
  public IReadOnlyList<string> TeamIds { get; init; } = [];
  public IReadOnlyList<string> CategoryIds { get; init; } = [];
  public string? CampaignId { get; init; }

  /// <summary>Release channel chosen at creation (0 = MainFeed, 1 = Patreon). Null = not recorded.</summary>
  public int? ReleaseChannel { get; init; }
}
