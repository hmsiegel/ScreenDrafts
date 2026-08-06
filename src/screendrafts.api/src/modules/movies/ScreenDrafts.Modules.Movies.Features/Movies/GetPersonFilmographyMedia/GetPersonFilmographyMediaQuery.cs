namespace ScreenDrafts.Modules.Movies.Features.Movies.GetPersonFilmographyMedia;

internal sealed record GetPersonFilmographyMediaQuery : IQuery<GetPersonFilmographyMediaResponse>
{
  public string ImdbId { get; init; } = default!;
}
