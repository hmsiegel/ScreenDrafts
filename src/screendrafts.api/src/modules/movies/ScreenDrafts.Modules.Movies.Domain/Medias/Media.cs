namespace ScreenDrafts.Modules.Movies.Domain.Medias;

#pragma warning disable CA1724
public sealed class Media : AggregateRoot<MediaId, Guid>
#pragma warning restore CA1724
{
  private readonly List<MediaGenre> _mediaGenres = [];
  private readonly List<MediaActor> _mediaActors = [];
  private readonly List<MediaDirector> _mediaDirectors = [];
  private readonly List<MediaWriter> _mediaWriters = [];
  private readonly List<MediaProducer> _mediaProducers = [];
  private readonly List<MediaProductionCompany> _mediaProductionCompanies = [];

  private Media(
    string publicId,
    string title,
    string year,
    string? plot,
    string? image,
    string? releaseDate,
    Uri? youtubeTrailerUrl,
    string? imdbId,
    int? tmdbId,
    int? igdbId,
    string? externalId,
    MediaType mediaType,
    int? tvSeriesTmdbId,
    int? seasonNumber,
    int? episodeNumber,
    MediaId? id = null)
    : base(id ?? MediaId.CreateUnique())
  {
    Title = Guard.Against.NullOrWhiteSpace(title);
    Year = Guard.Against.NullOrWhiteSpace(year);
    PublicId = Guard.Against.NullOrWhiteSpace(publicId);
    Plot = plot;
    Image = image;
    ReleaseDate = releaseDate;
    YoutubeTrailerUrl = youtubeTrailerUrl;
    TmdbId = tmdbId;
    ImdbId = imdbId;
    IgdbId = igdbId;
    ExternalId = externalId;
    MediaType = mediaType;
    TvSeriesTmdbId = tvSeriesTmdbId;
    SeasonNumber = seasonNumber;
    EpisodeNumber = episodeNumber;
  }

  private Media()
  {
  }

  public string PublicId { get; private set; } = default!;

  /// <summary>
  /// TMDbId - null for video games
  /// </summary>
  public int? TmdbId { get; private set; }
  
  /// <summary>
  /// IMDbId - nullable; not available for all media types
  /// </summary>
  public string? ImdbId { get; private set; } = default!;

  /// <summary>
  /// IGDB Id - only set for video games, null for movies and TV shows
  /// </summary>
  public int? IgdbId { get; private set; }

  /// <summary>
  /// Spare external ID for soucres outside TMDb and IGDB.
  /// </summary>
  public string? ExternalId { get; private set; }

  public string Title { get; private set; } = default!;

  public string Year { get; private set; } = default!;

  public string? Plot { get; private set; } = default!;

  public string? Image { get; private set; } = default!;

  public string? ReleaseDate { get; private set; } = default!;

  public Uri? YoutubeTrailerUrl { get; private set; } = default!;

  public MediaType MediaType { get; private set; } = MediaType.Movie;

  /// <summary>
  /// TMDb series ID - required for TV episodes to fetch episode credit credit
  /// </summary>
  public int? TvSeriesTmdbId { get; private set; }

  public int? SeasonNumber { get; private set; }
  public int? EpisodeNumber { get; private set; }

  public IReadOnlyCollection<MediaGenre> MediaGenres => _mediaGenres.AsReadOnly();

  public IReadOnlyCollection<MediaActor> MediaActors => _mediaActors.AsReadOnly();

  public IReadOnlyCollection<MediaDirector> MediaDirectors => _mediaDirectors.AsReadOnly();

  public IReadOnlyCollection<MediaWriter> MediaWriters => _mediaWriters.AsReadOnly();
  public IReadOnlyCollection<MediaProducer> MediaProducers => _mediaProducers.AsReadOnly();

  public IReadOnlyCollection<MediaProductionCompany> MediaProductionCompanies => _mediaProductionCompanies.AsReadOnly();

  public void AddGenre(MediaGenre genre)
  {
    _mediaGenres.Add(genre);
  }

  public void AddActor(MediaActor actor)
  {
    _mediaActors.Add(actor);
  }

  public void AddDirector(MediaDirector director)
  {
    _mediaDirectors.Add(director);
  }

  public void AddWriter(MediaWriter writer)
  {
    _mediaWriters.Add(writer);
  }

  public void AddProducer(MediaProducer producer)
  {
    _mediaProducers.Add(producer);
  }

  public void AddProductionCompany(MediaProductionCompany productionCompany)
  {
    _mediaProductionCompanies.Add(productionCompany);
  }

  public static Result<Media> Create(
    string publicId,
    string title,
    string year,
    string? plot,
    string? image,
    string? releaseDate,
    Uri? youtubeTrailerUrl,
    string? imdbId,
    int? tmdbId,
    int? igdbId,
    string? externalId,
    MediaType mediaType,
    int? tvSeriesTmdbId = null,
    int? seasonNumber = null,
    int? episodeNumber = null,
    MediaId? id = null)
  {
    if (string.IsNullOrWhiteSpace(title))
    {
      return Result.Failure<Media>(MediaErrors.RequiredFieldsMissing);
    }

    if (string.IsNullOrWhiteSpace(publicId))
    {
      return Result.Failure<Media>(MediaErrors.PublicIdRequired);
    }

    if (mediaType == MediaType.VideoGame && igdbId is null)
    {
      return Result.Failure<Media>(MediaErrors.IgdbIdRequiredForVideoGames);
    }

    if (mediaType != MediaType.VideoGame 
      && mediaType != MediaType.MusicVideo
      && tmdbId is null)
    {
      return Result.Failure<Media>(MediaErrors.TmdbIdRequired);
    }

    if (mediaType == MediaType.TvEpisode &&
      (tvSeriesTmdbId is null || seasonNumber is null || episodeNumber is null))
    {
      return Result.Failure<Media>(MediaErrors.EpisodeFieldsRequired);
    }

    var media = new Media(
      publicId: publicId,
      title: title,
      year: year,
      plot: plot,
      image: image,
      releaseDate: releaseDate,
      youtubeTrailerUrl: youtubeTrailerUrl,
      imdbId: imdbId,
      tmdbId: tmdbId,
      igdbId: igdbId,
      externalId: externalId,
      mediaType: mediaType,
      tvSeriesTmdbId: tvSeriesTmdbId,
      seasonNumber: seasonNumber,
      episodeNumber: episodeNumber,
      id: id);

    media.Raise(new MediaCreatedDomainEvent(
      mediaId: media.Id.Value,
      imdbId: imdbId,
      tmdbId: tmdbId,
      igdbId: igdbId,
      publicId: publicId,
      mediaType: mediaType));

    return Result.Success(media);
  }

  internal void SetTmdbId(int tmdbId) => TmdbId = tmdbId;
  internal void SetMediaType(MediaType mediaType) => MediaType = mediaType;
  internal void SetEpisodeDetails(int tvSeriesTmdbId, int seasonNumber, int episodeNumber)
  {
    TvSeriesTmdbId = tvSeriesTmdbId;
    SeasonNumber = seasonNumber;
    EpisodeNumber = episodeNumber;
  }

  internal void SetPublicId(string publicId)
  {
    ArgumentNullException.ThrowIfNull(publicId);
    PublicId = publicId;
  }
}
