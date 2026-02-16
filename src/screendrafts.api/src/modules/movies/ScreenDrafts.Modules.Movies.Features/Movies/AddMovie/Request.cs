namespace ScreenDrafts.Modules.Movies.Features.Movies.AddMovie;

internal sealed record Request(
  string ImdbId,
  string Title,
  string Year,
  string Plot,
  string Image,
  string ReleaseDate,
  string YoutubeTrailer,
  IReadOnlyCollection<string> Genres,
  IReadOnlyCollection<PersonRequest> Actors,
  IReadOnlyCollection<PersonRequest> Directors,
  IReadOnlyCollection<PersonRequest> Writers,
  IReadOnlyCollection<PersonRequest> Producers,
  IReadOnlyCollection<ProductionCompanyRequest> ProductionCompanies);
