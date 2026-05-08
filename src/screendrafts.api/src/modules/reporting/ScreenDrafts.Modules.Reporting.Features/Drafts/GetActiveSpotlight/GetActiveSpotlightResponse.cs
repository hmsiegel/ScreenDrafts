namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetActiveSpotlight;

internal sealed record GetActiveSpotlightResponse
{
  public string DraftPublicId { get; init; } = default!;
  public string Title { get; init; } = default!;
  public int? EpisodeNumber { get; init; }
  public string DraftType { get; init; } = default!;
  public int TotalParts { get; init; }
  public string SpotlightDescription { get; init; } = default!;
  public string? SpotifyUrl { get; init; }
  public IReadOnlyList<SpotlightPickResponse> TopPicks { get; init; } = [];
}
