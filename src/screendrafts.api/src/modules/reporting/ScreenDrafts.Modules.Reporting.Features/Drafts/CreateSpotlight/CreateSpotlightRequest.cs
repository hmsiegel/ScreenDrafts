namespace ScreenDrafts.Modules.Reporting.Features.Drafts.CreateSpotlight;

internal sealed record CreateSpotlightRequest
{
  public required string DraftPublicId { get; init; }
  public required string SpotlightDescription { get; init; }
  public string? SpotifyUrl { get; init; }
}
