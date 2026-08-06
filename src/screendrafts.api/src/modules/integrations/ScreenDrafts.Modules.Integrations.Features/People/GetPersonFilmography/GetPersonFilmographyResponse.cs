namespace ScreenDrafts.Modules.Integrations.Features.People.GetPersonFilmography;

internal sealed record GetPersonFilmographyResponse
{
  public string PersonName { get; init; } = string.Empty;
  public string? PersonPhotoUrl { get; init; }
  public IReadOnlyList<FilmographyCreditResponse> Credits { get; init; } = [];
}
