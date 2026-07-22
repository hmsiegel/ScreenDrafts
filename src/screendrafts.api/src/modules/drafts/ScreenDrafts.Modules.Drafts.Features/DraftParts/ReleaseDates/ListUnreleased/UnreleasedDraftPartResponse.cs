namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.ReleaseDates.ListUnreleased;

internal sealed record UnreleasedDraftPartResponse
{
  public required string DraftPartPublicId { get; init; }
  public required string DraftPublicId { get; init; }
  public required string DraftTitle { get; init; }
  public int PartIndex { get; init; }
  public int DraftType { get; init; }
  public required string SeriesPublicId { get; init; }
  public required string SeriesName { get; init; }
}
