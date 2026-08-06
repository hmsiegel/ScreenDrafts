namespace ScreenDrafts.Modules.Movies.Infrastructure.Medias;

internal sealed class MediaRepository(MoviesDbContext context) : IMediaRepository
{
  private readonly MoviesDbContext _context = context;

  public void Add(Media media)
  {
    _context.Media.Add(media);
  }

  public void Update(Media media)
  {
    _context.Media.Update(media);
  }

  public void AddMediaActor(Media media, Person actor)
  {
    _context.MediaActors.Add(MediaActor.Create(media.Id, actor.Id));
  }

  public void AddMediaDirector(Media media, Person director)
  {
    _context.MediaDirectors.Add(MediaDirector.Create(media.Id, director.Id));
  }

  public void AddMediaGenre(Media media, Genre genre)
  {
    _context.MediaGenres.Add(MediaGenre.Create(media.Id, genre.Id));
  }

  public void AddMediaProducer(Media media, Person producer)
  {
    _context.MediaProducers.Add(MediaProducer.Create(media.Id, producer.Id));
  }

  public void AddMediaProductionCompany(Media media, ProductionCompany productionCompany)
  {
    _context.MediaProductionCompanies.Add(
      MediaProductionCompany.Create(media.Id, productionCompany.Id)
    );
  }

  public void AddMediaWriter(Media media, Person writer)
  {
    _context.MediaWriters.Add(MediaWriter.Create(media.Id, writer.Id));
  }

  public async Task<bool> ExistsByIgdbIdAsync(
    int igdbId,
    CancellationToken cancellationToken = default
  )
  {
    return await _context.Media.AnyAsync(m => m.IgdbId == igdbId, cancellationToken);
  }

  public async Task<bool> ExistsByTmdbIdAsync(
    int tmdbId,
    MediaType mediaType,
    CancellationToken cancellationToken = default
  )
  {
    return await _context.Media.AnyAsync(
      m => m.TmdbId == tmdbId && m.MediaType == mediaType,
      cancellationToken
    );
  }

  public async Task<Media?> FindByImdbIdAsync(
    string imdbId,
    CancellationToken cancellationToken = default
  )
  {
    return await _context
      .Media.Include(m => m.MediaGenres)
      .Include(m => m.MediaActors)
      .Include(m => m.MediaDirectors)
      .Include(m => m.MediaWriters)
      .Include(m => m.MediaProducers)
      .Include(m => m.MediaProductionCompanies)
      .SingleOrDefaultAsync(m => m.ImdbId == imdbId, cancellationToken);
  }

  public async Task<Media?> FindByTmdbIdAsync(
    int tmdbId,
    MediaType mediaType,
    CancellationToken cancellationToken = default
  )
  {
    return await _context
      .Media.Include(m => m.MediaGenres)
      .Include(m => m.MediaActors)
      .Include(m => m.MediaDirectors)
      .Include(m => m.MediaWriters)
      .Include(m => m.MediaProducers)
      .Include(m => m.MediaProductionCompanies)
      .SingleOrDefaultAsync(m => m.TmdbId == tmdbId && m.MediaType == mediaType, cancellationToken);
  }

  public async Task<Media?> FindByIgdbIdAsync(
    int igdbId,
    CancellationToken cancellationToken = default
  )
  {
    return await _context
      .Media.Include(m => m.MediaGenres)
      .Include(m => m.MediaActors)
      .Include(m => m.MediaDirectors)
      .Include(m => m.MediaWriters)
      .Include(m => m.MediaProducers)
      .Include(m => m.MediaProductionCompanies)
      .SingleOrDefaultAsync(m => m.IgdbId == igdbId, cancellationToken);
  }

  public async Task<HashSet<string>> GetExistingMediaImdbsAsync(
    IEnumerable<string> imdbIds,
    CancellationToken cancellationToken = default
  )
  {
    return
    [
      .. await _context
        .Media.Where(m => m.ImdbId != null && imdbIds.Contains(m.ImdbId))
        .Select(m => m.ImdbId!)
        .ToListAsync(cancellationToken),
    ];
  }

  public async Task<HashSet<int>> GetExistingMediaIgdbsAsync(
    IEnumerable<int> igdbIds,
    CancellationToken cancellationToken = default
  )
  {
    return
    [
      .. await _context
        .Media.Where(m => m.IgdbId != null && igdbIds.Contains(m.IgdbId.Value))
        .Select(m => m.IgdbId!.Value)
        .ToListAsync(cancellationToken),
    ];
  }

  public async Task<HashSet<(int TmdbId, int MediaTypeValue)>> GetExistingMediaTmdbsAsync(
    IEnumerable<int> tmdbIds,
    CancellationToken cancellationToken = default
  )
  {
    var rows = await _context
      .Media.Where(m => m.TmdbId != null && tmdbIds.Contains(m.TmdbId.Value))
      .Select(m => new { m.TmdbId, MediaTypeValue = m.MediaType.Value })
      .ToListAsync(cancellationToken);

    return [.. rows.Select(m => (m.TmdbId!.Value, m.MediaTypeValue))];
  }

  public async Task<
    Dictionary<(int TmdbId, int MediaTypeValue), string>
  > GetPublicIdsByTmdbIdsAsync(
    IEnumerable<(int TmdbId, int MediaTypeValue)> tmdbIds,
    CancellationToken cancellationToken = default
  )
  {
    var pairs = tmdbIds as ICollection<(int TmdbId, int MediaTypeValue)> ?? [.. tmdbIds];
    var distinctTmdbIds = pairs.Select(p => p.TmdbId).Distinct().ToArray();

    var rows = await _context
      .Media.Where(m => m.TmdbId != null && distinctTmdbIds.Contains(m.TmdbId.Value))
      .Select(m => new
      {
        m.TmdbId,
        MediaTypeValue = m.MediaType.Value,
        m.PublicId,
      })
      .ToListAsync(cancellationToken);

    var pairSet = pairs.ToHashSet();

    return rows.Where(m => pairSet.Contains((m.TmdbId!.Value, m.MediaTypeValue)))
      .ToDictionary(m => (m.TmdbId!.Value, m.MediaTypeValue), m => m.PublicId!);
  }

  public async Task<Media?> FindByTvEpisodeAsync(
    int tvSeriesTmdbId,
    int seasonNumber,
    int episodeNumber,
    CancellationToken cancellationToken = default
  )
  {
    return await _context
      .Media.Include(m => m.MediaGenres)
      .Include(m => m.MediaActors)
      .Include(m => m.MediaDirectors)
      .Include(m => m.MediaWriters)
      .Include(m => m.MediaProducers)
      .Include(m => m.MediaProductionCompanies)
      .SingleOrDefaultAsync(
        m =>
          m.TvSeriesTmdbId == tvSeriesTmdbId
          && m.SeasonNumber == seasonNumber
          && m.EpisodeNumber == episodeNumber,
        cancellationToken
      );
  }

  public async Task<Media?> FindByTmdbIdForUpdateAsync(
    int tmdbId,
    MediaType mediaType,
    CancellationToken cancellationToken = default
  )
  {
    return await _context.Media.SingleOrDefaultAsync(
      m => m.TmdbId == tmdbId && m.MediaType == mediaType,
      cancellationToken
    );
  }

  public async Task<Media?> FindByTvEpisodeForUpdateAsync(
    int tvSeriesTmdbId,
    int seasonNumber,
    int episodeNumber,
    CancellationToken cancellationToken = default
  )
  {
    return await _context.Media.SingleOrDefaultAsync(
      m =>
        m.TvSeriesTmdbId == tvSeriesTmdbId
        && m.SeasonNumber == seasonNumber
        && m.EpisodeNumber == episodeNumber,
      cancellationToken
    );
  }

  public async Task<bool> ExistsByExternalIdAsync(
    string externalId,
    CancellationToken cancellationToken = default
  )
  {
    return await _context.Media.AnyAsync(m => m.ExternalId == externalId, cancellationToken);
  }

  public async Task<HashSet<string>> GetExistingMediaExternalIdsAsync(
    IEnumerable<string> externalIds,
    CancellationToken cancellationToken = default
  )
  {
    var rows = await _context
      .Media.Where(m => m.ExternalId != null && externalIds.Contains(m.ExternalId))
      .Select(m => m.ExternalId!)
      .ToListAsync(cancellationToken);

    return [.. rows];
  }

  public async Task<Dictionary<string, string>> GetPublicIdsByImdbIdsAsync(
    IEnumerable<string> imdbIds,
    CancellationToken cancellationToken = default
  )
  {
    var rows = await _context
      .Media.Where(m => m.ImdbId != null && imdbIds.Contains(m.ImdbId))
      .Select(m => new { m.ImdbId, m.PublicId })
      .ToListAsync(cancellationToken);

    return rows.ToDictionary(m => m.ImdbId!, m => m.PublicId!);
  }

  public async Task<Dictionary<int, string>> GetPublicIdsByIgdbIdsAsync(
    IEnumerable<int> igdbIds,
    CancellationToken cancellationToken = default
  )
  {
    var rows = await _context
      .Media.Where(m => m.IgdbId != null && igdbIds.Contains(m.IgdbId.Value))
      .Select(m => new { m.IgdbId, m.PublicId })
      .ToListAsync(cancellationToken);

    return rows.ToDictionary(m => m.IgdbId!.Value, m => m.PublicId!);
  }

  public async Task<Dictionary<string, string>> GetPublicIdsByExternalIdsAsync(
    IEnumerable<string> externalIds,
    CancellationToken cancellationToken = default
  )
  {
    var rows = await _context
      .Media.Where(m => m.ExternalId != null && externalIds.Contains(m.ExternalId))
      .Select(m => new { m.ExternalId, m.PublicId })
      .ToListAsync(cancellationToken);

    return rows.ToDictionary(m => m.ExternalId!, m => m.PublicId!);
  }
}
