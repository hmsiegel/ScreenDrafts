using ScreenDrafts.Common.Abstractions.Results;

namespace ScreenDrafts.Modules.Movies.Features.Movies.AddMovie;

internal sealed class AddMovieCommandHandler(
  IMovieRepository movieRepository,
  IUnitOfWork unitOfWork,
  IGenreRepository genreRepository,
  IPersonRepository personRepository,
  IProductionCompanyRepository productionCompanyRepository,
  ILogger<AddMovieCommandHandler> logger)
  : ICommandHandler<AddMovieCommand, string>
{
  private readonly IMovieRepository _movieRepository = movieRepository;
  private readonly IGenreRepository _genreRepository = genreRepository;
  private readonly IPersonRepository _personRepository = personRepository;
  private readonly IProductionCompanyRepository _productionCompanyRepository = productionCompanyRepository;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;
  private readonly ILogger<AddMovieCommandHandler> _logger = logger;

  public async Task<Result<string>> Handle(AddMovieCommand request, CancellationToken cancellationToken)
  {
    // Check if the movie exists in the database
    var movieExists = await _movieRepository.ExistsAsync(request.ImdbId, cancellationToken);

    if (movieExists)
    {
      MovieLoggingMessages.MovieAlreadyExists(_logger, request.ImdbId);
      return Result.Failure<string>(MovieErrors.MovieAlreadyExists(request.ImdbId));
    }

    var releaseDate = request.ReleaseDate is not null ? request.ReleaseDate : request.Year;
    var year = request.Year is not null ? request.Year : request.ReleaseDate!.Split('-')[0];
    var plot = request.Plot is not null ? request.Plot : string.Empty;

    Uri? youtubeTrailerUrl = null!;
    if (request.YouTubeTrailerUrl is not null)
    {
      youtubeTrailerUrl = request.YouTubeTrailerUrl;
    }

    var movie = Movie.Create(
      request.Title,
      year!,
      plot,
      request.Image,
      releaseDate,
      youtubeTrailerUrl,
      request.ImdbId);

    if (movie.IsFailure)
    {
      MovieLoggingMessages.CreateMovieFailed(_logger, movie.Error!.ToString());
      return Result.Failure<string>(movie.Error);
    }

    _movieRepository.Add(movie.Value);
    MovieLoggingMessages.MovieAddedToDatabase(_logger, request.Title, request.ImdbId);

    var peopleCache = new Dictionary<string, Person>();

    async Task<Person> GetOrCreatePerson(string name, string imdbId)
    {
      if (peopleCache.TryGetValue(imdbId, out var existingPerson))
      {
        return existingPerson;
      }

      var person = await _personRepository.FindByImdbIdAsync(imdbId, cancellationToken);

      if (person is null)
      {
        person = Person.Create(imdbId, name);
        _personRepository.Add(person);
      }

      peopleCache[imdbId] = person;
      return person;
    }

    // Add the genres
    foreach (var genreName in request.Genres)
    {
      var genre = await _genreRepository.FindByNameAsync(genreName, cancellationToken);

      if (genre is null)
      {
        genre = Genre.Create(genreName);
        _genreRepository.Add(genre);
      }

      _movieRepository.AddMovieGenre(movie.Value, genre);
    }

    // Add the directors
    if (request.Directors is not null)
    {
      foreach (var director in request.Directors)
      {
        var person = await GetOrCreatePerson(director.Name, director.ImdbId);
        _movieRepository.AddMovieDirector(movie.Value, person);
      }
    }

    // Add the actors
    if (request.Actors is not null)
    {
      foreach (var actor in request.Actors)
      {
        var person = await GetOrCreatePerson(actor.Name, actor.ImdbId);
        _movieRepository.AddMovieActor(movie.Value, person);
      }
    }

    // Add the writers
    if (request.Writers is not null)
    {
      foreach (var writer in request.Writers)
      {
        var person = await GetOrCreatePerson(writer.Name, writer.ImdbId);
        _movieRepository.AddMovieWriter(movie.Value, person);
      }
    }

    // Add the producers
    if (request.Producers is not null)
    {
      foreach (var producer in request.Producers)
      {
        var person = await GetOrCreatePerson(producer.Name, producer.ImdbId);
        _movieRepository.AddMovieProducer(movie.Value, person);
      }
    }

    // Add the production companies
    if (request.ProductionCompanies is not null)
    {
      foreach (var company in request.ProductionCompanies)
      {
        var productionCompany = await _productionCompanyRepository.FindByImdbIdAsync(company.ImdbId, cancellationToken);

        if (productionCompany is null)
        {
          productionCompany = ProductionCompany.Create(company.Name, company.ImdbId);
          _productionCompanyRepository.Add(productionCompany);
        }
        else
        {
          var existingEntity = _productionCompanyRepository.FindExistingEntity(company.ImdbId, cancellationToken);

          if (existingEntity is not null)
          {
            productionCompany = existingEntity;
          }
          else
          {
            _productionCompanyRepository.Attach(productionCompany);
          }
        }

        if (!await _productionCompanyRepository.RelationshipExistsAsync(movie.Value.Id.Value, productionCompany.Id, cancellationToken))
        {
          _movieRepository.AddMovieProductionCompany(movie.Value, productionCompany);
        }
      }
    }

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    return movie.Value.ImdbId;
  }
}

