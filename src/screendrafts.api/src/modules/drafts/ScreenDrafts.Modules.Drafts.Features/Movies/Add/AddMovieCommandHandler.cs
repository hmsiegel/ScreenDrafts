namespace ScreenDrafts.Modules.Drafts.Features.Movies.Add;

internal sealed class AddMovieCommandHandler(IMovieRepository movieRepository)
  : ICommandHandler<AddMovieCommand, string>
{
  private readonly IMovieRepository _movieRepository = movieRepository;

  public async Task<Result<string>> Handle(AddMovieCommand request, CancellationToken cancellationToken)
  {
    var exists = await _movieRepository.ExistsByPublicIdAsync(request.PublicId, cancellationToken);

    if (exists)
    {
      return Result.Failure<string>(MovieErrors.MovieAlreadyExists(request.PublicId));
    }

    var result = Movie.Create(
      movieTitle: request.Title,
      publicId: request.PublicId,
      id: request.Id,
      imdbId: request.ImdbId,
      tmdbId: request.TmdbId,
      igdbId: request.IgdbId,
      mediaType: request.MediaType);

    if (result.IsFailure)
    {
      return Result.Failure<string>(result.Errors);
    }

    var movie = result.Value;

    _movieRepository.Add(movie);

    return Result.Success(movie.PublicId);
  }
}


