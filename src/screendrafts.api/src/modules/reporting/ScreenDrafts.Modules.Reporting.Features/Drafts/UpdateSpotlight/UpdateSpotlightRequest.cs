namespace ScreenDrafts.Modules.Reporting.Features.Drafts.UpdateSpotlight;

// ── Request ───────────────────────────────────────────────────────────────

// PublicId binds from the route; the other two bind from the body.
internal sealed record UpdateSpotlightRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;
  public required string SpotlightDescription { get; init; }
  public string? SpotifyUrl { get; init; }
}
