namespace ScreenDrafts.Modules.Movies.Features.Movies.AddMovie;

internal sealed record AddMovieCommand(
  string ImdbId,
  int TmdbId,
  string Title,
  string Year,
  string Plot,
  string Image,
  string? ReleaseDate,
  Uri? YouTubeTrailerUrl,
  IReadOnlyCollection<GenreRequest> Genres,
  IReadOnlyCollection<PersonRequest>? Directors,
  IReadOnlyCollection<PersonRequest>? Actors,
  IReadOnlyCollection<PersonRequest>? Writers,
  IReadOnlyCollection<PersonRequest>? Producers,
  IReadOnlyCollection<ProductionCompanyRequest>? ProductionCompanies)
  : ICommand<string>;
