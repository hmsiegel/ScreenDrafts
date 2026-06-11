namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetSpotlights;

internal sealed record SpotlightListItem
{
  public required string PublicId { get; init; }
  public required string DraftPublicId { get; init; }
  public required string Title { get; init; }
  public required string DraftType { get; init; }
  public required int? EpisodeNumber { get; init; }
  public required string SpotlightDescription { get; init; }
  public required Uri? SpotifyUrl { get; init; }
  public required bool IsActive { get; init; }
  public required bool IsPinned { get; init; }
  public required DateTime? ActivatedAtUtc { get; init; }
  public required DateTime CreatedAtUtc { get; init; }
}
