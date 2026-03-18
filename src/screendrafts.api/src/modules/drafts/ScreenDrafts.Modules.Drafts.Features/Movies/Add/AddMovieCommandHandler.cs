namespace ScreenDrafts.Modules.Drafts.Features.Movies.Add;

internal sealed class AddMovieCommandHandler(IMovieRepository movieRepository)
  : ICommandHandler<AddMovieCommand, string>
{
  private readonly IMovieRepository _movieRepository = movieRepository;

  public async Task<Result<string>> Handle(AddMovieCommand request, CancellationToken cancellationToken)
  {
    var movieExists = await _movieRepository.ExistsByImdbIdAsync(request.ImdbId, cancellationToken);

    if (movieExists)
    {
      return Result.Failure<string>(MovieErrors.MovieAlreadyExists(request.ImdbId));
    }

    var result = Movie.Create(
      movieTitle: request.Title,
      imdbId: request.ImdbId,
      id: request.Id,
      tmdbId: request.TmdbId);

    if (result.IsFailure)
    {
      return Result.Failure<string>(result.Errors);
    }

    var movie = result.Value;

    _movieRepository.Add(movie);

    return Result.Success(movie.ImdbId);
  }
}


