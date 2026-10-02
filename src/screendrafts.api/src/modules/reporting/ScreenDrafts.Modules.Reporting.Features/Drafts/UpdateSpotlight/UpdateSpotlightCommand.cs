namespace ScreenDrafts.Modules.Reporting.Features.Drafts.UpdateSpotlight;

// ── Command ───────────────────────────────────────────────────────────────

internal sealed record UpdateSpotlightCommand : ICommand
{
  public required string PublicId { get; init; }
  public required string SpotlightDescription { get; init; }

  // null or whitespace clears the link.
  public string? SpotifyUrl { get; init; }
}
