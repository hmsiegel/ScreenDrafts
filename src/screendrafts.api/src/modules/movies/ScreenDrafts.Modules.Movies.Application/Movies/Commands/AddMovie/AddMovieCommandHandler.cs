namespace ScreenDrafts.Modules.Movies.Application.Movies.Commands.AddMovie;

internal sealed class AddMovieCommandHandler(
  IMovieRepository movieRepository,
  IUnitOfWork unitOfWork,
  IGenreRepository genreRepository,
  IPersonRepository personRepository,
  IProductionCompanyRepository productionCompanyRepository)
  : ICommandHandler<AddMovieCommand, Guid>
{
  private readonly IMovieRepository _movieRepository = movieRepository;
  private readonly IGenreRepository _genreRepository = genreRepository;
  private readonly IPersonRepository _personRepository = personRepository;
  private readonly IProductionCompanyRepository _productionCompanyRepository = productionCompanyRepository;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;

  public async Task<Result<Guid>> Handle(AddMovieCommand request, CancellationToken cancellationToken)
  {
    await _unitOfWork.BeginTransactionAsync(cancellationToken);
    // Check if the movie exeists in the database
    var movieExists = await _movieRepository.ExistsAsync(request.ImdbId, cancellationToken);

    if (movieExists)
    {
      await _unitOfWork.RollbackTransactionAsync(cancellationToken);
      return Result.Failure<Guid>(MovieErrors.MovieAlreadyExists(request.ImdbId));
    }

    var movie = Movie.Create(
      request.Title,
      request.Year,
      request.Plot,
      request.Image,
      request.ReleaseDate,
      request.YouTubeTrailerUrl,
      request.ImdbId).Value;

    _movieRepository.Add(movie);

    // Add the genres
    foreach (var genreName in request.Genres)
    {
      var genre = await _genreRepository.FindByNameAsync(genreName, cancellationToken)
        ?? Genre.Create(genreName);

      _genreRepository.Add(genre);
      _movieRepository.AddMovieGenre(movie, genre);
    }

    // Add the directors
    foreach (var director in request.Directors)
    {
      var person = await _personRepository.FindByImdbIdAsync(director.ImdbId, cancellationToken)
        ?? Person.Create(director.Name, director.ImdbId);

      _personRepository.Add(person);
      _movieRepository.AddMovieDirector(movie, person);
    }

    // Add the actors
    foreach (var actor in request.Actors)
    {
      var person = await _personRepository.FindByImdbIdAsync(actor.ImdbId, cancellationToken)
        ?? Person.Create(actor.Name, actor.ImdbId);
      _personRepository.Add(person);
      _movieRepository.AddMovieActor(movie, person);
    }

    // Add the writers
    foreach (var writer in request.Writers)
    {
      var person = await _personRepository.FindByImdbIdAsync(writer.ImdbId, cancellationToken)
        ?? Person.Create(writer.Name, writer.ImdbId);
      _personRepository.Add(person);
      _movieRepository.AddMovieWriter(movie, person);
    }

    // Add the producers
    foreach (var producer in request.Producers)
    {
      var person = await _personRepository.FindByImdbIdAsync(producer.ImdbId, cancellationToken)
        ?? Person.Create(producer.Name, producer.ImdbId);
      _personRepository.Add(person);
      _movieRepository.AddMovieProducer(movie, person);
    }

    // Add the production companies
    foreach (var company in request.ProductionCompanies)
    {
      var productionCompany = await _productionCompanyRepository.FindByImdbIdAsync(company.ImdbId, cancellationToken)
        ?? ProductionCompany.Create(company.Name, company.ImdbId);
      _productionCompanyRepository.Add(productionCompany);
      _movieRepository.AddMovieProductionCompany(movie, productionCompany);
    }

    await _unitOfWork.SaveChangesAsync(cancellationToken);
    await _unitOfWork.CommitTransactionAsync(cancellationToken);

    return movie.Id.Value;
  }
}

