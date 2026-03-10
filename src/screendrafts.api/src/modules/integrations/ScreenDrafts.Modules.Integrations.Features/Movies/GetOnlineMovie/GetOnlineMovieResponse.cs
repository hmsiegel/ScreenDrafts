namespace ScreenDrafts.Modules.Integrations.Features.Movies.GetOnlineMovie;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1002:Do not expose generic lists", Justification = "<Pending>")]
internal sealed record GetOnlineMovieResponse(
  string ImdbId,
  int TmdbId,
  string Title,
  string Year,
  string Plot,
  string Image,
  string? ReleaseDate,
  Uri? YouTubeTrailerUri,
  List<GenreModel> Genres,
  List<ActorModel> Actors,
  List<DirectorModel> Directors,
  List<WriterModel> Writers,
  List<ProducerModel> Producers,
  List<ProductionCompanyModel> ProductionCompanies);
