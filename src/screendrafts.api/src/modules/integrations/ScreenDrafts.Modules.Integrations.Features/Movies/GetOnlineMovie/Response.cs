namespace ScreenDrafts.Modules.Integrations.Features.Movies.GetOnlineMovie;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1002:Do not expose generic lists", Justification = "<Pending>")]
internal sealed record Response(
  string ImdbId,
  string Title,
  string Year,
  string Plot,
  string Image,
  string? ReleaseDate,
  Uri? YouTubeTrailerUri,
  List<string> Genres,
  List<ActorModel> Actors,
  List<DirectorModel> Directors,
  List<WriterModel> Writers,
  List<ProducerModel> Producers,
  List<ProductionCompanyModel> ProductionCompanies);
