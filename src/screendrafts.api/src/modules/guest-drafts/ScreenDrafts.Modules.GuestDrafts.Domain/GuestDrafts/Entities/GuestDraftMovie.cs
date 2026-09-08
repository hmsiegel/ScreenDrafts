namespace ScreenDrafts.Modules.GuestDrafts.Domain.GuestDrafts.Entities;

/// <summary>
/// Local cache of a Movies-module Media, mirroring canonical Drafts' Movie
/// entity. Populated by MediaAddedIntegrationEventConsumer whenever the
/// Movies module publishes MediaAddedIntegrationEvent -- Id and PublicId are
/// the SAME values as the source Media (not independently generated), so a
/// GuestDraftMovie's identity is always traceable back to its Movies-module
/// origin.
///
/// Deliberately drops Movie.cs's Versions/Picks navigation -- GuestDrafts has
/// no re-release/version-tracking concept and GuestDraftPick already holds
/// its own MovieId FK, so no inverse navigation is needed here.
/// </summary>
public sealed class GuestDraftMovie : Entity
{
  private GuestDraftMovie(
    string publicId,
    string movieTitle,
    string? imdbId,
    int? tmdbId,
    int? igdbId,
    MediaType mediaType,
    string? year,
    Guid id,
    int? tvSeriesTmdbId,
    int? seasonNumber,
    int? episodeNumber,
    string? tvSeriesTitle
  )
    : base(id)
  {
    PublicId = publicId;
    MovieTitle = movieTitle;
    ImdbId = imdbId;
    TmdbId = tmdbId;
    IgdbId = igdbId;
    MediaType = mediaType;
    Year = year;
    TvSeriesTmdbId = tvSeriesTmdbId;
    SeasonNumber = seasonNumber;
    EpisodeNumber = episodeNumber;
    TvSeriesTitle = tvSeriesTitle;
  }

  private GuestDraftMovie() { }

  public string PublicId { get; private set; } = default!;
  public string MovieTitle { get; private set; } = default!;
  public string? ImdbId { get; private set; }
  public int? TmdbId { get; private set; }
  public int? IgdbId { get; private set; }
  public MediaType MediaType { get; private set; } = default!;
  public string? Year { get; private set; }

  /// <summary>
  /// TMDb series ID. Only set when MediaType is TvEpisode — mirrors the field of
  /// the same name on the Movies module's Media aggregate, synced across via
  /// MediaAddedIntegrationEvent.
  /// </summary>
  public int? TvSeriesTmdbId { get; private set; }
  public int? SeasonNumber { get; private set; }
  public int? EpisodeNumber { get; private set; }

  /// <summary>
  /// Display name of the parent series (e.g. "Star Trek: The Original Series").
  /// Only set when MediaType is TvEpisode.
  /// </summary>
  public string? TvSeriesTitle { get; private set; }

  public static Result<GuestDraftMovie> Create(
    string movieTitle,
    string publicId,
    MediaType mediaType,
    Guid id,
    string? imdbId = null,
    int? tmdbId = null,
    int? igdbId = null,
    string? year = null,
    int? tvSeriesTmdbId = null,
    int? seasonNumber = null,
    int? episodeNumber = null,
    string? tvSeriesTitle = null
  )
  {
    if (string.IsNullOrWhiteSpace(movieTitle))
    {
      return Result.Failure<GuestDraftMovie>(GuestDraftMovieErrors.InvalidMovieTitle);
    }

    if (string.IsNullOrWhiteSpace(publicId))
    {
      return Result.Failure<GuestDraftMovie>(GuestDraftMovieErrors.InvalidPublicId);
    }

    var movie = new GuestDraftMovie(
      movieTitle: movieTitle,
      publicId: publicId,
      imdbId: imdbId,
      id: id,
      tmdbId: tmdbId,
      igdbId: igdbId,
      mediaType: mediaType,
      year: year,
      tvSeriesTmdbId: tvSeriesTmdbId,
      seasonNumber: seasonNumber,
      episodeNumber: episodeNumber,
      tvSeriesTitle: tvSeriesTitle
    );
    return movie;
  }
}
