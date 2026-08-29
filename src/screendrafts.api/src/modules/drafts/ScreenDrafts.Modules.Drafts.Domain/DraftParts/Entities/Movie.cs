namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Entities;

public sealed class Movie : Entity
{
  private readonly List<Pick> _picks = [];
  private readonly List<MovieVersion> _versions = [];

  private Movie(
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

  private Movie() { }

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

  public bool HasDefinedVersions => _versions.Count > 0;
  public IReadOnlyCollection<Pick> Picks => _picks.AsReadOnly();
  public IReadOnlyCollection<MovieVersion> Versions => _versions.AsReadOnly();

  public static Result<Movie> Create(
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
      return Result.Failure<Movie>(MovieErrors.InvalidMovieTitle);
    }

    if (string.IsNullOrWhiteSpace(publicId))
    {
      return Result.Failure<Movie>(MovieErrors.InvalidPublicId);
    }

    var movie = new Movie(
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

  public void AddVersion(MovieVersion version)
  {
    if (!_versions.Any(v => v.Name.Equals(version.Name, StringComparison.OrdinalIgnoreCase)))
    {
      _versions.Add(version);
    }
  }

  public bool TryNormalizeVersionName(string input, out string canonicalName)
  {
    canonicalName = null!;

    if (string.IsNullOrWhiteSpace(input))
    {
      return false;
    }

    var match = _versions.FirstOrDefault(v =>
      v.Name.Equals(input.Trim(), StringComparison.OrdinalIgnoreCase)
    );

    if (match is null)
    {
      return false;
    }

    canonicalName = match.Name;
    return true;
  }
}
