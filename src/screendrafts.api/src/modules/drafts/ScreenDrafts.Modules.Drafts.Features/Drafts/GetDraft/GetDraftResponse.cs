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
  public int? EpisodeNumber { get; init; }
  public string? ImagePath { get; init; }
  public string? CampaignPublicId { get; init; }
  public string? CampaignName { get; init; }
  public string? FungibleTokenName { get; init; }
  public bool IsHostless { get; init; }

  /// <summary>
  /// TMDb series ID this draft is restricted to, if any — see Draft.RestrictedTvSeriesTmdbId
  /// on the domain entity. Editable only while DraftStatus is Created (see
  /// SetTvSeriesRestrictionCommandHandler); the frontend should treat any
  /// other status as read-only for this pair of fields.
  /// </summary>
  public int? RestrictedTvSeriesTmdbId { get; init; }
  public string? RestrictedTvSeriesTitle { get; init; }
  public IReadOnlyList<GetDraftCategoryResponse>? Categories { get; init; } = [];
  public IReadOnlyList<GetDraftPartResponse> Parts { get; init; } = [];
}
