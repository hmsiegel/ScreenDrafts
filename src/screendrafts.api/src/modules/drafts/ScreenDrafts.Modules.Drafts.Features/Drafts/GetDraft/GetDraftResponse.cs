namespace ScreenDrafts.Modules.Drafts.Features.Drafts.GetDraft;

internal sealed record GetDraftResponse
{
  public required string PublicId { get; init; }
  public required string Title { get; init; }
  public string? Description { get; init; }
  public int DraftType { get; init; }
  public int DraftStatus { get; init; }
  public required string SeriesPublicId { get; init; }
  public required string SeriesName { get; init; }
  public string? CampaignPublicId { get; init; }
  public string? CampaignName { get; init; }
  public IReadOnlyList<GetDraftPartResponse> Parts { get; init; } = [];
}
