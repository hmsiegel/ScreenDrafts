namespace ScreenDrafts.Modules.Integrations.PublicApi;

public sealed record PersonFilmographyApiResponse
{
  public string PersonName { get; init; } = default!;
  public string? PersonPhotoPath { get; init; }
  public IReadOnlyList<PersonFilmographyCreditApiResult> Credits { get; init; } = [];
}
