namespace ScreenDrafts.Modules.Integrations.Features.People.GetPersonFilmography;

// ── Request ───────────────────────────────────────────────────────────────────

internal sealed record GetPersonFilmographyRequest
{
  [FromQuery(Name = "imdbId")]
  public string ImdbId { get; init; } = default!;
}
