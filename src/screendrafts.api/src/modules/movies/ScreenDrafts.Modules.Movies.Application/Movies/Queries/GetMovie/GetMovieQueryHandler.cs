namespace ScreenDrafts.Modules.Movies.Application.Movies.Queries.GetMovie;

internal sealed class GetMovieQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<GetMovieQuery, MovieResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<MovieResponse>> Handle(GetMovieQuery request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql =
      $"""
      select
      	m.id AS {nameof(MovieResponse.Id)},
      	m.title AS {nameof(MovieResponse.Title)},
      	m.year AS {nameof(MovieResponse.Year)},
      	m.plot AS {nameof(MovieResponse.Plot)},
      	m.image AS {nameof(MovieResponse.Image)},
      	m.release_date AS {nameof(MovieResponse.ReleaseDate)},
      	m.youtube_trailer_url AS {nameof(MovieResponse.YouTubeTrailer)},
      	m.imdb_id AS {nameof(MovieResponse.ImdbId)},
      	array_remove(array_agg(distinct g.name), null) AS {nameof(MovieResponse.Genres)},
      	array_remove(array_agg(distinct a.name), null) AS {nameof(MovieResponse.Actors)},
      	array_remove(array_agg(distinct d.name), null) AS {nameof(MovieResponse.Directors)},
      	array_remove(array_agg(distinct w.name), null) AS {nameof(MovieResponse.Writers)},
      	array_remove(array_agg(distinct p.name), null) AS {nameof(MovieResponse.Producers)},
      	array_remove(array_agg(distinct pc.name), null) AS {nameof(MovieResponse.ProductionCompanies)}
      from movies.movies m 
      left join movies.movie_genres mg on mg.movie_id = m.id
      left join movies.genres g on g.id = mg.genre_id
      left join movies.movie_actors ma on ma.movie_id = m.id
      left join movies.people a on a.id = ma.actor_id
      left join movies.movie_directors md on md.movie_id = m.id
      left join movies.people d on d.id = md.director_id 
      left join movies.movie_writers mw on mw.movie_id = m.id
      left join movies.people w on w.id = mw.writer_id
      left join movies.movie_producers mp on mp.movie_id = m.id
      left join movies.people p on p.id = mp.producer_id
      left join movies.movie_production_companies mpc on mpc.movie_id = m.id
      left join movies.production_companies pc on pc.id = mpc.production_company_id
      where m.imdb_id = @ImdbId
      group by m.id;
      """;

    var parameters = new { request.ImdbId };

    var movieDictionary = new Dictionary<Guid, MovieResponse>();

    await connection.QueryAsync<MovieResponse, GenreResponse, ActorResponse, DirectorResponse, WriterResponse, ProducerResponse, ProductionCompanyResponse, MovieResponse>(
      sql,
      (movie, genre, actor, director, writer, producer, productionCompany) =>
      {
        if (!movieDictionary.TryGetValue(movie.Id, out var movieEntry))
        {
          movieEntry = new MovieResponse(
            movie.Id,
            movie.ImdbId,
            movie.Title,
            movie.Year,
            movie.Plot,
            movie.Image,
            movie.ReleaseDate,
            movie.YouTubeTrailer);

          movieDictionary[movie.Id] = movieEntry;
        }

        if (genre is not null)
          movieEntry!.AddGenre(genre);

        if (actor is not null && !string.IsNullOrEmpty(actor.ImdbId))
          movieEntry!.AddActor(actor);

        if (director is not null && !string.IsNullOrEmpty(director.ImdbId))
          movieEntry!.AddDirector(director);

        if (writer is not null && !string.IsNullOrEmpty(writer.ImdbId))
          movieEntry!.AddWriter(writer);

        if (producer is not null && !string.IsNullOrEmpty(producer.ImdbId))
          movieEntry!.AddProducer(producer);

        if (productionCompany is not null && !string.IsNullOrEmpty(productionCompany.ImdbId))
          movieEntry!.AddProductionCompany(productionCompany);

        return movieEntry!;
      },
      parameters,
      splitOn: "Genres, Actors, Directors, Writers, Producers, ProductionCompanies"
    );

    var movie = movieDictionary.Values.FirstOrDefault();

    if (movie is null)
    {
      return Result.Failure<MovieResponse>(MovieErrors.MovieNotFound(request.ImdbId));
    }

    return movie;
  }
}
