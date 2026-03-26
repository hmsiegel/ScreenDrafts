namespace ScreenDrafts.Modules.Drafts.Features.Drafts.GetDraft;

internal sealed record GetDraftResponse
{
  public required string PublicId { get; init; }
  public required string Title { get; init; }
  public string? Description { get; init; }
  public DraftType DraftType { get; init; } = default!;
  public DraftStatus DraftStatus { get; init; } = default!;
  public required string SeriesPublicId { get; init; }
  public required string SeriesName { get; init; }
  public string? CampaignPublicId { get; init; }
  public string? CampaignName { get; init; }
  public IReadOnlyList<GetDraftPartResponse> Parts { get; init; } = [];
}
