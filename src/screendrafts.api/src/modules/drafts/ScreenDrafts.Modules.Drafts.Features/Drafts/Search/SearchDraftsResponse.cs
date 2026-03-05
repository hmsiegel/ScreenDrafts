namespace ScreenDrafts.Modules.Drafts.Features.Drafts.Search;

internal sealed record SearchDraftsResponse
{
  public required string PublicId { get; init; }
  public required string Title { get; init; }
  public int DraftType { get; init; }
  public int DraftStatus { get; init; }
  public string? CampaignPublicId { get; init; }
  public string? CampaignName { get; init; }
  public required string SeriesPublicId { get; init; }
  public required string SeriesName { get; init; }
}
