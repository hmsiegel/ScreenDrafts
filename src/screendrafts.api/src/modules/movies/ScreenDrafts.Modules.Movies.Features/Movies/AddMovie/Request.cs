namespace ScreenDrafts.Modules.Movies.Features.Movies.AddMovie;

internal sealed record Request(
  string ImdbId,
  int TmdbId,
  string Title,
  string Year,
  string Plot,
  string Image,
  string ReleaseDate,
  string YoutubeTrailer,
  IReadOnlyCollection<GenreRequest> Genres,
  IReadOnlyCollection<PersonRequest> Actors,
  IReadOnlyCollection<PersonRequest> Directors,
  IReadOnlyCollection<PersonRequest> Writers,
  IReadOnlyCollection<PersonRequest> Producers,
  IReadOnlyCollection<ProductionCompanyRequest> ProductionCompanies);
