namespace ScreenDrafts.Modules.Movies.Infrastructure.Database.Seeding;

internal sealed class MovieSeeder(
  ILogger<MovieSeeder> logger,
  ICsvFileService csvFileService,
  IMovieRepository movieRepository,
  IEventBus eventBus,
  IDateTimeProvider dateTimeProvider,
  MoviesDbContext dbContext)
  : ICustomSeeder
{
  private readonly ILogger<MovieSeeder> _logger = logger;
  private readonly ICsvFileService _csvFileService = csvFileService;
  private readonly IEventBus _eventBus = eventBus;
  private readonly MoviesDbContext _dbContext = dbContext;
  private readonly IMovieRepository _movieRepository = movieRepository;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    var dataPath = Environment.GetEnvironmentVariable("DATA_PATH")
      ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
    var moviesExportCsv = Path.Combine(dataPath, FileNames.MovieExportSeeder);
    var moviesCsv = Path.Combine(dataPath, FileNames.MovieSeeder);
    var genresCsv = Path.Combine(dataPath, FileNames.GenreSeeder);
    var peopleExportCsv = Path.Combine(dataPath, FileNames.PersonSeeder);
    var productionCompaniesCsv = Path.Combine(dataPath, FileNames.ProductionCompanySeeder);
    var movieGenresCsv = Path.Combine(dataPath, FileNames.MovieGenreSeeder);
    var movieActorsCsv = Path.Combine(dataPath, FileNames.MovieActorsSeeder);
    var movieDirectorsCsv = Path.Combine(dataPath, FileNames.MovieDirectorsSeeder);
    var movieWritersCsv = Path.Combine(dataPath, FileNames.MovieWritersSeeder);
    var movieProductionCompaniesCsv = Path.Combine(dataPath, FileNames.MovieProductionCompaniesSeeder);
    var movieProducersCsv = Path.Combine(dataPath, FileNames.MovieProducersSeeder);

    await SeedMoviesExportAsync(moviesExportCsv, cancellationToken);
    await SeedGenresExportAsync(genresCsv, cancellationToken);
    await SeedPeopleExortAsync(peopleExportCsv, cancellationToken);
    await SeedProductionCompaniesExportAsync(productionCompaniesCsv, cancellationToken);
    await SeedMoviesAndActorsAsync(movieActorsCsv, cancellationToken);
    await SeedMoviesAndDirectorsAsync(movieDirectorsCsv, cancellationToken);
    await SeedMoviesAndWritersAsync(movieWritersCsv, cancellationToken);
    await SeedMoviesAndProducersAsync(movieProducersCsv, cancellationToken);
    await SeedMoviesAndGenresAsync(movieGenresCsv, cancellationToken);
    await SeedMoviesAndProductionCompaniesAsync(movieProductionCompaniesCsv, cancellationToken);
    await SeedMoviesImdbAsync(moviesCsv, cancellationToken);
  }

  private async Task SeedMoviesExportAsync(string filePath, CancellationToken cancellationToken)
  {
    const string TableName = "Movies";

    DatabaseSeedingLoggingMessages.StartingSeeding(_logger, TableName);

    if (!File.Exists(filePath))
    {
      DatabaseSeedingLoggingMessages.FileNotFound(_logger, filePath);
      return;
    }

    var csvMovies = _csvFileService.ReadCsvFile<MovieExportCsvModel>(filePath).ToList();

    if (csvMovies is null)
    {
      return;
    }

    var movieIds = csvMovies.Select(movie => movie.ImdbId).ToList();
    var existingMovieIds = await _dbContext.Movies
      .Where(movie => movieIds.Contains(movie.ImdbId))
      .Select(movie => movie.ImdbId)
      .ToHashSetAsync(cancellationToken);

    var newMovies = csvMovies.Where(movie => !existingMovieIds.Contains(movie.ImdbId)).ToList();

    if (newMovies.Count == 0)
    {
      DatabaseSeedingLoggingMessages.AlreadySeeded(_logger, TableName);
      return;
    }

    foreach (var newMovie in newMovies)
    {
      var uri = string.IsNullOrWhiteSpace(newMovie.YouTubeTrailerUrl)
        ? null
        : new Uri(newMovie.YouTubeTrailerUrl);

      var movie = Movie.Create(
        newMovie.Title,
        newMovie.Year,
        newMovie.Plot,
        newMovie.Image,
        newMovie.ReleaseDate,
        uri,
        newMovie.ImdbId,
        MovieId.Create(Guid.Parse(newMovie.Id))).Value;
      _dbContext.Movies.Add(movie);
    }

    await _dbContext.SaveChangesAsync(cancellationToken);

    DatabaseSeedingLoggingMessages.BulkInsertMessage(_logger, newMovies.Count, filePath, TableName);

    DatabaseSeedingLoggingMessages.SeedingComplete(_logger, TableName);
  }

  private async Task SeedGenresExportAsync(string filePath, CancellationToken cancellationToken)
  {
    const string TableName = "Genres";

    if (!File.Exists(filePath))
    {
      DatabaseSeedingLoggingMessages.FileNotFound(_logger, filePath);
      return;
    }

    var csvGenres = _csvFileService.ReadCsvFile<GenreExportCsvModel>(filePath).ToList();

    if (csvGenres is null)
    {
      return;
    }

    var genreIds = csvGenres.Select(genre => Guid.Parse(genre.Id)).ToList();
    var existingGenreIds = await _dbContext.Genres
      .Where(genre => genreIds.Contains(genre.Id))
      .Select(genre => genre.Id)
      .ToHashSetAsync(cancellationToken);

    var newGenres = csvGenres.Where(genre => !existingGenreIds.Contains(Guid.Parse(genre.Id))).ToList();

    if (newGenres.Count == 0)
    {
      DatabaseSeedingLoggingMessages.AlreadySeeded(_logger, TableName);
      return;
    }

    foreach (var newGenre in newGenres)
    {
      var genre = Genre.Create(
        newGenre.Name,
        Guid.Parse(newGenre.Id));

      _dbContext.Genres.Add(genre);
    }
    await _dbContext.SaveChangesAsync(cancellationToken);
    DatabaseSeedingLoggingMessages.BulkInsertMessage(_logger, newGenres.Count, filePath, TableName);
  }

  private async Task SeedPeopleExortAsync(string filePath, CancellationToken cancellationToken)
  {
    const string TableName = "People";

    if (!File.Exists(filePath))
    {
      DatabaseSeedingLoggingMessages.FileNotFound(_logger, filePath);
      return;
    }

    var csvPeople = _csvFileService.ReadCsvFile<PeopleExportCsvModel>(filePath).ToList();

    if (csvPeople is null)
    {
      return;
    }

    var personIds = csvPeople.Select(person => PersonId.Create(Guid.Parse(person.Id))).ToList();
    var existingPersonIds = await _dbContext.People
      .Where(person => personIds.Contains(person.Id))
      .Select(person => person.Id)
      .ToHashSetAsync(cancellationToken);

    var newPeople = csvPeople.Where(person => !existingPersonIds.Contains(PersonId.Create(Guid.Parse(person.Id)))).ToList();

    if (newPeople.Count == 0)
    {
      DatabaseSeedingLoggingMessages.AlreadySeeded(_logger, TableName);
      return;
    }

    foreach (var newPerson in newPeople)
    {
      var person = Person.Create(
        newPerson.ImdbId,
        newPerson.Name,
        PersonId.Create(Guid.Parse(newPerson.Id)));
      _dbContext.People.Add(person);

    }
    await _dbContext.SaveChangesAsync(cancellationToken);

    DatabaseSeedingLoggingMessages.BulkInsertMessage(_logger, newPeople.Count, filePath, TableName);
  }

  private async Task SeedProductionCompaniesExportAsync(string filePath, CancellationToken cancellationToken)
  {
    const string TableName = "ProductionCompanies";

    if (!File.Exists(filePath))
    {
      DatabaseSeedingLoggingMessages.FileNotFound(_logger, filePath);
      return;
    }

    var csvProductionCompanies = _csvFileService.ReadCsvFile<ProductionCompanyExportCsvModel>(filePath).ToList();

    if (csvProductionCompanies is null)
    {
      return;
    }

    var productionCompanyIds = csvProductionCompanies.Select(productionCompany =>Guid.Parse(productionCompany.Id)).ToList();
    var existingProductionCompanyIds = await _dbContext.ProductionCompanies
      .Where(productionCompany => productionCompanyIds.Contains(productionCompany.Id))
      .Select(productionCompany => productionCompany.Id)
      .ToHashSetAsync(cancellationToken);

    var newProductionCompanies = csvProductionCompanies
      .Where(productionCompany => !existingProductionCompanyIds.Contains(Guid.Parse(productionCompany.Id)))
      .ToList();

    if (newProductionCompanies.Count == 0)
    {
      DatabaseSeedingLoggingMessages.AlreadySeeded(_logger, TableName);
      return;
    }

    foreach (var newProductionCompany in newProductionCompanies)
    {
      var productionCompany = ProductionCompany.Create(
        newProductionCompany.Name,
        newProductionCompany.ImdbId,
        Guid.Parse(newProductionCompany.Id));

      _dbContext.ProductionCompanies.Add(productionCompany);
    }

    await _dbContext.SaveChangesAsync(cancellationToken);

    DatabaseSeedingLoggingMessages.BulkInsertMessage(_logger, newProductionCompanies.Count, filePath, TableName);
  }

  private async Task SeedMoviesAndActorsAsync(string filePath, CancellationToken cancellationToken)
  {
    const string TableName = "MovieActors";

    DatabaseSeedingLoggingMessages.StartingSeeding(_logger, TableName);

    if (!File.Exists(filePath))
    {
      DatabaseSeedingLoggingMessages.FileNotFound(_logger, filePath);
      return;
    }

    var csvMovieActors = await File.ReadAllLinesAsync(filePath, cancellationToken);

    foreach (var line in csvMovieActors.Skip(1))
    {
      var values = line.Split(',');

      if (values.Length < 2)
      {
        continue;
      }

      var movieId = Guid.Parse(values[0].Trim());
      var actorId = Guid.Parse(values[1].Trim());
      var id = Guid.CreateVersion7();

      await _dbContext.Database.ExecuteSqlRawAsync(
        """
        INSERT INTO movies.movie_actors (movie_id, actor_id, id)
        VALUES ({0}, {1}, {2})
        ON CONFLICT DO NOTHING
        """,
        movieId,
        actorId,
        id);

      DatabaseSeedingLoggingMessages.BulkInsertMessage(_logger, csvMovieActors.Length, filePath, TableName);
    }
  }

  private async Task SeedMoviesAndDirectorsAsync(string filePath, CancellationToken cancellationToken)
  {
    const string TableName = "MovieDirectors";

    DatabaseSeedingLoggingMessages.StartingSeeding(_logger, TableName);

    if (!File.Exists(filePath))
    {
      DatabaseSeedingLoggingMessages.FileNotFound(_logger, filePath);
      return;
    }

    var csvMovieDirectors = await File.ReadAllLinesAsync(filePath, cancellationToken);

    foreach (var line in csvMovieDirectors.Skip(1))
    {
      var values = line.Split(',');

      if (values.Length < 2)
      {
        continue;
      }

      var movieId = Guid.Parse(values[0].Trim());
      var directorId = Guid.Parse(values[1].Trim());
      var id = Guid.CreateVersion7();

      await _dbContext.Database.ExecuteSqlRawAsync(
        """
        INSERT INTO movies.movie_directors (movie_id, director_id, id)
        VALUES ({0}, {1}, {2})
        ON CONFLICT DO NOTHING
        """,
        movieId,
        directorId,
        id);

      DatabaseSeedingLoggingMessages.BulkInsertMessage(_logger, csvMovieDirectors.Length, filePath, TableName);
    }
  }

  private async Task SeedMoviesAndWritersAsync(string filePath, CancellationToken cancellationToken)
  {
    const string TableName = "MoviesWriters";

    DatabaseSeedingLoggingMessages.StartingSeeding(_logger, TableName);

    if (!File.Exists(filePath))
    {
      DatabaseSeedingLoggingMessages.FileNotFound(_logger, filePath);
      return;
    }

    var csvMovieWriters = await File.ReadAllLinesAsync(filePath, cancellationToken);

    foreach (var line in csvMovieWriters.Skip(1))
    {
      var values = line.Split(',');

      if (values.Length < 2)
      {
        continue;
      }

      var movieId = Guid.Parse(values[0].Trim());
      var writerId = Guid.Parse(values[1].Trim());
      var id = Guid.CreateVersion7();

      await _dbContext.Database.ExecuteSqlRawAsync(
        """
        INSERT INTO movies.movie_writers (movie_id, writer_id, id)
        VALUES ({0}, {1}, {2})
        ON CONFLICT DO NOTHING
        """,
        movieId,
        writerId,
        id);

      DatabaseSeedingLoggingMessages.BulkInsertMessage(_logger, csvMovieWriters.Length, filePath, TableName);
    }
  }

  private async Task SeedMoviesAndProducersAsync(string filePath, CancellationToken cancellationToken)
  {
    const string TableName = "MoviesProducers";

    DatabaseSeedingLoggingMessages.StartingSeeding(_logger, TableName);

    if (!File.Exists(filePath))
    {
      DatabaseSeedingLoggingMessages.FileNotFound(_logger, filePath);
      return;
    }

    var csvMovieProducers = await File.ReadAllLinesAsync(filePath, cancellationToken);

    foreach (var line in csvMovieProducers.Skip(1))
    {
      var values = line.Split(',');

      if (values.Length < 2)
      {
        continue;
      }

      var movieId = Guid.Parse(values[0].Trim());
      var producerId = Guid.Parse(values[1].Trim());
      var id = Guid.CreateVersion7();

      await _dbContext.Database.ExecuteSqlRawAsync(
        """
        INSERT INTO movies.movie_producers (movie_id, producer_id, id)
        VALUES ({0}, {1}, {2})
        ON CONFLICT DO NOTHING
        """,
        movieId,
        producerId,
        id);

      DatabaseSeedingLoggingMessages.BulkInsertMessage(_logger, csvMovieProducers.Length, filePath, TableName);
    }
  }
  private async Task SeedMoviesAndGenresAsync(string filePath, CancellationToken cancellationToken)
  {
    const string TableName = "MoviesGenres";

    DatabaseSeedingLoggingMessages.StartingSeeding(_logger, TableName);

    if (!File.Exists(filePath))
    {
      DatabaseSeedingLoggingMessages.FileNotFound(_logger, filePath);
      return;
    }

    var csvMovieGenres = await File.ReadAllLinesAsync(filePath, cancellationToken);

    foreach (var line in csvMovieGenres.Skip(1))
    {
      var values = line.Split(',');

      if (values.Length < 2)
      {
        continue;
      }

      var movieId = Guid.Parse(values[0].Trim());
      var genreId = Guid.Parse(values[1].Trim());

      await _dbContext.Database.ExecuteSqlRawAsync(
        """
        INSERT INTO movies.movie_genres (movie_id, genre_id)
        VALUES ({0}, {1})
        ON CONFLICT DO NOTHING
        """,
        movieId,
        genreId);

      DatabaseSeedingLoggingMessages.BulkInsertMessage(_logger, csvMovieGenres.Length, filePath, TableName);
    }
  }
  private async Task SeedMoviesAndProductionCompaniesAsync(string filePath, CancellationToken cancellationToken)
  {
    const string TableName = "MoviesProductionCompanies";

    DatabaseSeedingLoggingMessages.StartingSeeding(_logger, TableName);

    if (!File.Exists(filePath))
    {
      DatabaseSeedingLoggingMessages.FileNotFound(_logger, filePath);
      return;
    }

    var csvMovieProductionCompanies = await File.ReadAllLinesAsync(filePath, cancellationToken);

    foreach (var line in csvMovieProductionCompanies.Skip(1))
    {
      var values = line.Split(',');

      if (values.Length < 2)
      {
        continue;
      }

      var movieId = Guid.Parse(values[0].Trim());
      var productionCompanyId = Guid.Parse(values[1].Trim());

      await _dbContext.Database.ExecuteSqlRawAsync(
        """
          INSERT INTO movies.movie_production_companies (movie_id, production_company_id)
          VALUES ({0}, {1})
          ON CONFLICT DO NOTHING
          """,
          movieId,
        productionCompanyId);

      DatabaseSeedingLoggingMessages.BulkInsertMessage(_logger, csvMovieProductionCompanies.Length, filePath, TableName);
    }
  }

  private async Task SeedMoviesImdbAsync(string filePath, CancellationToken cancellationToken)
  {
    const string TableName = "Movies";

    if (!File.Exists(filePath))
    {
      DatabaseSeedingLoggingMessages.FileNotFound(_logger, filePath);
      return;
    }

    var csvMovies = _csvFileService.ReadCsvFile<MovieCsvModel>(filePath).ToList();

    if (csvMovies is null)
    {
      return;
    }

    var movieIds = csvMovies.Select(movie => movie.ImdbId).ToList();

    var existingMovieIds = await _movieRepository.GetExistingMovieImdbsAsync(movieIds, cancellationToken);

    var newMovies = csvMovies.Where(movie => !existingMovieIds.Contains(movie.ImdbId)).ToList();

    if (newMovies.Count == 0)
    {
      DatabaseSeedingLoggingMessages.AlreadySeeded(_logger, TableName);
      return;
    }

    foreach (var newMovie in newMovies)
    {
      await _eventBus.PublishAsync(
        new FetchMovieRequestedIntegrationEvent(
          Guid.NewGuid(),
          _dateTimeProvider.UtcNow,
          newMovie.ImdbId),
        cancellationToken);
    }
  }
}
