namespace ScreenDrafts.Modules.Drafts.Features.Movies.Add;

internal sealed class AddMovieCommandHandler(IMovieRepository movieRepository)
  : ICommandHandler<AddMovieCommand, string>
{
  private readonly IMovieRepository _movieRepository = movieRepository;

  public async Task<Result<string>> Handle(AddMovieCommand request, CancellationToken cancellationToken)
  {
    var movieExists = await _movieRepository.ExistsAsync(request.Id, cancellationToken);

    if (movieExists)
    {
      return Result.Failure<string>(MovieErrors.MovieAlreadyExists(request.ImdbId));
    }

    var result = Movie.Create(request.Title, request.ImdbId, request.Id);

    if (result.IsFailure)
    {
      return Result.Failure<string>(result.Errors);
    }

    var movie = result.Value;

    _movieRepository.Add(movie);

    return Result.Success(movie.ImdbId);
  }
}


