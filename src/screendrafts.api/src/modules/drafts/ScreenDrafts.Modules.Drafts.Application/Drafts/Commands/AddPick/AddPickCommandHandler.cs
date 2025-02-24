namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddPick;

internal sealed class AddPickCommandHandler(
  IDraftsRepository draftsRepository,
  IPicksRepository picksRepository,
  IUnitOfWork unitOfWork) 
  : ICommandHandler<AddPickCommand>
{
  private readonly IDraftsRepository _draftsRepository = draftsRepository;
  private readonly IPicksRepository _picksRepository = picksRepository;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;
  public async Task<Result> Handle(AddPickCommand request, CancellationToken cancellationToken)
  {
    var draftId = DraftId.Create(request.DraftId);

    var draft = await _draftsRepository.GetByIdAsync(draftId, cancellationToken);

    if (draft is null)
    {
      return Result.Failure<Guid>(DraftErrors.NotFound(request.DraftId));
    }

    if (draft.DraftStatus != DraftStatus.InProgress)
    {
      return Result.Failure<Guid>(DraftErrors.DraftNotStarted);
    }

    var drafter = draft.Drafters.FirstOrDefault(d => d.Id.Value == request.DrafterId);

    if (drafter is null)
    {
      return Result.Failure<Guid>(DrafterErrors.NotFound(request.DrafterId));
    }

    var movie = await _draftsRepository.GetMovieByIdAsync(request.MovieId, cancellationToken);

    if (movie is null)
    {
      return Result.Failure<Guid>(DraftErrors.MovieNotFound(request.MovieId));
    }

    var result = draft.AddPick(request.Position, movie, drafter);

    if (result.IsFailure)
    {
      return Result.Failure(result.Errors);
    }

    _picksRepository.Add(draft.Picks.Last());
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success();
  }
}
