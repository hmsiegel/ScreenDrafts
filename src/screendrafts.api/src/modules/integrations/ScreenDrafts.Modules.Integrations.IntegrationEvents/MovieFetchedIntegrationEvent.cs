using ScreenDrafts.Common.Application.EventBus;

namespace ScreenDrafts.Modules.Integrations.IntegrationEvents;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1002:Do not expose generic lists", Justification = "<Pending>")]
public sealed class MovieFetchedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  string imdbId,
  string title,
  string year,
  string plot,
  string image,
  string? releaseDate,
  Uri? youTubeTrailerUri,
  List<string> genres,
  List<ActorModel> actors,
  List<DirectorModel> directors,
  List<WriterModel> writers,
  List<ProducerModel> producers,
  List<ProductionCompanyModel> productionCompanies) : IntegrationEvent(id, occurredOnUtc)
{
  public string ImdbId { get; init; } = imdbId;

  public string Title { get; init; } = title;

  public string Year { get; init; } = year;

  public string Plot { get; init; } = plot;

  public string Image { get; init; } = image;

  public string? ReleaseDate { get; init; } = releaseDate;

  public Uri? YouTubeTrailerUri { get; init; } = youTubeTrailerUri;

  public List<string> Genres { get; init; } = genres;

  public List<ActorModel> Actors { get; init; } = actors;

  public List<DirectorModel> Directors { get; init; } = directors;

  public List<WriterModel> Writers { get; init; } = writers;

  public List<ProducerModel> Producers { get; init; } = producers;

  public List<ProductionCompanyModel> ProductionCompanies { get; init; } = productionCompanies;
}
