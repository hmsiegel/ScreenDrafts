using ScreenDrafts.Common.Application.EventBus;
using ScreenDrafts.Common.Domain;

namespace ScreenDrafts.Modules.Integrations.IntegrationEvents;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1002:Do not expose generic lists", Justification = "Integration event requires lists for serialization")]
public sealed class MediaFetchedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  string publicId,
  string? imdbId,
  int? tmdbId,
  int? igdbId,
  string title,
  string year,
  string? plot,
  string? image,
  string? releaseDate,
  Uri? youTubeTrailerUri,
  MediaType mediaType,
  int? tvSeriesTmdbId,
  int? seasonNumber,
  int? episodeNumber,
  List<GenreModel> genres,
  List<ActorModel> actors,
  List<DirectorModel> directors,
  List<WriterModel> writers,
  List<ProducerModel> producers,
  List<ProductionCompanyModel> productionCompanies)
  : IntegrationEvent(id, occurredOnUtc)
{
  public string PublicId { get; init; } = publicId;
  public string? ImdbId { get; init; } = imdbId;
  public int? TmdbId { get; init; } = tmdbId;
  public int? IgdbId { get; init; } = igdbId;
  public string Title { get; init; } = title;
  public string Year { get; init; } = year;
  public string? Plot { get; init; } = plot;
  public string? Image { get; init; } = image;
  public string? ReleaseDate { get; init; } = releaseDate;
  public Uri? YouTubeTrailerUri { get; init; } = youTubeTrailerUri;

  public MediaType MediaType { get; init; } = mediaType;

  public int? TVSeriesTmdbId { get; init; } = tvSeriesTmdbId;
  public int? SeasonNumber { get; init; } = seasonNumber;
  public int? EpisodeNumber { get; init; } = episodeNumber;

  public List<GenreModel> Genres { get; init; } = genres;

  public List<ActorModel> Actors { get; init; } = actors;

  public List<DirectorModel> Directors { get; init; } = directors;

  public List<WriterModel> Writers { get; init; } = writers;

  public List<ProducerModel> Producers { get; init; } = producers;

  public List<ProductionCompanyModel> ProductionCompanies { get; init; } = productionCompanies;
}
