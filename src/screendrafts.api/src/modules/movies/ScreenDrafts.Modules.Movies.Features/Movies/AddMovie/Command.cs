namespace ScreenDrafts.Modules.Movies.Features.Movies.AddMovie;

internal sealed record Command(
  string ImdbId,
  string Title,
  string Year,
  string Plot,
  string Image,
  string? ReleaseDate,
  Uri? YouTubeTrailerUrl,
  IReadOnlyCollection<string> Genres,
  IReadOnlyCollection<PersonRequest>? Directors,
  IReadOnlyCollection<PersonRequest>? Actors,
  IReadOnlyCollection<PersonRequest>? Writers,
  IReadOnlyCollection<PersonRequest>? Producers,
  IReadOnlyCollection<ProductionCompanyRequest>? ProductionCompanies)
  : ICommand<string>;
