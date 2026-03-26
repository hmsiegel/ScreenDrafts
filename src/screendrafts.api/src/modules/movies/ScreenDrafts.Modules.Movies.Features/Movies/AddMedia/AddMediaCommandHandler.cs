namespace ScreenDrafts.Modules.Movies.Features.Movies.AddMedia;

internal sealed class AddMediaCommandHandler(
  IMediaRepository mediaRepository,
  IGenreRepository genreRepository,
  IPersonRepository personRepository,
  IProductionCompanyRepository productionCompanyRepository,
  ILogger<AddMediaCommandHandler> logger)
  : ICommandHandler<AddMediaCommand, string>
{
  private readonly IMediaRepository _mediaRepository = mediaRepository;
  private readonly IGenreRepository _genreRepository = genreRepository;
  private readonly IPersonRepository _personRepository = personRepository;
  private readonly IProductionCompanyRepository _productionCompanyRepository = productionCompanyRepository;
  private readonly ILogger<AddMediaCommandHandler> _logger = logger;

  public async Task<Result<string>> Handle(AddMediaCommand request, CancellationToken cancellationToken)
  {
    // Existence check: use TmdbId for movies & tv, Igdb for games.
    var titleExists = request.IgdbId.HasValue
      ? await _mediaRepository.ExistsByIgdbIdAsync(request.IgdbId.Value, cancellationToken)
      : await _mediaRepository.ExistsByTmdbIdAsync(request.TmdbId!.Value, request.MediaType, cancellationToken);

    if (titleExists)
    {
      var existingId = request.IgdbId?.ToString(CultureInfo.InvariantCulture) ?? request.TmdbId?.ToString(CultureInfo.InvariantCulture);
      MovieLoggingMessages.MovieAlreadyExists(_logger, existingId!);
      return Result.Failure<string>(MediaErrors.MediaAlreadyExists(existingId!));
    }

    var releaseDate = request.ReleaseDate ?? request.Year;
    var year = request.Year ?? request.ReleaseDate!.Split('-')[0];
    var plot = request.Plot ?? string.Empty;

    var mediaResult = Media.Create(
      publicId: request.PublicId,
      title: request.Title,
      releaseDate: releaseDate,
      year: year,
      plot: plot,
      image: request.Image,
      imdbId: request.ImdbId,
      tmdbId: request.TmdbId,
      igdbId: request.IgdbId,
      externalId: null,
      youtubeTrailerUrl: request.YouTubeTrailerUrl,
      mediaType: request.MediaType,
      tvSeriesTmdbId: request.TvSeriesTmdbId,
      seasonNumber: request.SeasonNumber,
      episodeNumber: request.EpisodeNumber);

    if (mediaResult.IsFailure)
    {
      MovieLoggingMessages.CreateMovieFailed(_logger, mediaResult.Error!.ToString());
      return Result.Failure<string>(mediaResult.Error!);
    }

    var media = mediaResult.Value;
    _mediaRepository.Add(media);
    MovieLoggingMessages.MovieAddedToDatabase(_logger, media.Title, request.ImdbId 
      ?? request.TmdbId?.ToString(CultureInfo.InvariantCulture) 
      ?? request.IgdbId?.ToString(CultureInfo.InvariantCulture) 
      ?? "N/A");

    var peopleCache = new Dictionary<string, Person>();

    async Task<Person?> GetOrCreatePersonAsync(string name, string imdbId, int tmdbId)
    {
      if (string.IsNullOrWhiteSpace(imdbId))
      {
        return null;
      }

      if (peopleCache.TryGetValue(imdbId, out var existing))
      {
        return existing;
      }

      var person = await _personRepository.FindByImdbIdAsync(imdbId, cancellationToken);

      if (person is null)
      {
        person = Person.Create(imdbId, name, tmdbId);
        _personRepository.Add(person);
      }

      peopleCache[imdbId] = person;
      return person;
    }

    foreach (var genreRequest in request.Genres)
    {
      var genre = await _genreRepository.FindByNameAsync(genreRequest.Name, cancellationToken);

      if (genre is null)
      {
        genre = Genre.Create(genreRequest.Name, genreRequest.TmdbId);
        _genreRepository.Add(genre);
      }

      _mediaRepository.AddMediaGenre(media, genre);
    }

    if (request.Directors is not null)
    {
      foreach (var d in request.Directors)
      {
        var person = await GetOrCreatePersonAsync(d.Name, d.ImdbId, d.TmdbId);
        if (person is not null)
        {
          _mediaRepository.AddMediaDirector(media, person);
        }
      }
    }

    if (request.Actors is not null)
    {
      foreach (var a in request.Actors)
      {
        var person = await GetOrCreatePersonAsync(a.Name, a.ImdbId, a.TmdbId);
        if (person is not null)
        {
          _mediaRepository.AddMediaActor(media, person);
        }
      }
    }

    if (request.Producers is not null)
    {
      foreach (var p in request.Producers)
      {
        var person = await GetOrCreatePersonAsync(p.Name, p.ImdbId, p.TmdbId);
        if (person is not null)
        {
          _mediaRepository.AddMediaProducer(media, person);
        }
      }
    }

    if (request.Writers is not null)
    {
      foreach (var w in request.Writers)
      {
        var person = await GetOrCreatePersonAsync(w.Name, w.ImdbId, w.TmdbId);
        if (person is not null)
        {
          _mediaRepository.AddMediaWriter(media, person);
        }
      }
    }

    if (request.ProductionCompanies is not null)
    {
      foreach (var company in request.ProductionCompanies)
      {
        var productionCompany = await _productionCompanyRepository.FindByImdbIdAsync(company.ImdbId, cancellationToken);

        if (productionCompany is null)
        {
          productionCompany = ProductionCompany.Create(company.Name, company.ImdbId, company.TmdbId);
          _productionCompanyRepository.Add(productionCompany);
        }
        else
        {
          var existingEntity = _productionCompanyRepository
            .FindExistingEntity(company.ImdbId, cancellationToken);
          productionCompany = existingEntity ?? productionCompany;
          if (existingEntity is null)
          {
            _productionCompanyRepository.Add(productionCompany);
          }
        }
        if (!await _productionCompanyRepository.RelationshipExistsAsync(media.Id.Value, productionCompany.Id, cancellationToken))
        {
          _mediaRepository.AddMediaProductionCompany(media, productionCompany);
        }
      }
    }

    return Result.Success(media.ImdbId ?? media.TmdbId?.ToString(CultureInfo.InvariantCulture) ?? media.IgdbId?.ToString(CultureInfo.InvariantCulture) ?? "N/A");
  }
}

