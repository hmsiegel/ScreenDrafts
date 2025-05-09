namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddMovie;

internal sealed class AddMovieCommandHandler(IDraftsRepository draftsRepository) : ICommandHandler<AddMovieCommand, Guid>
{
  private readonly IDraftsRepository _draftsRepository = draftsRepository;

  public async Task<Result<Guid>> Handle(AddMovieCommand request, CancellationToken cancellationToken)
  {
    var movieExists = await _draftsRepository.MovieExistsAsync(request.ImdbId, cancellationToken);

    if (movieExists)
    {
      return Result.Failure<Guid>(DraftErrors.MovieAlreadyExists(request.ImdbId));
    }

    var result = Movie.Create(request.Title, request.ImdbId, request.Id);

    if (result.IsFailure)
    {
      return Result.Failure<Guid>(result.Error!);
    }

    var movie = result.Value;

    _draftsRepository.AddMovie(movie);

    return movie.Id;
  }
}
