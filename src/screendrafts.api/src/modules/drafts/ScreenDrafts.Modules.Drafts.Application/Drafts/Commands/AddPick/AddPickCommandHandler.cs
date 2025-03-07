namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddPick;

internal sealed class AddPickCommandHandler(
  IDraftsRepository draftsRepository,
  IPicksRepository picksRepository,
  IUnitOfWork unitOfWork)
  : ICommandHandler<AddPickCommand, Guid>
{
  private readonly IDraftsRepository _draftsRepository = draftsRepository;
  private readonly IPicksRepository _picksRepository = picksRepository;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;

  public async Task<Result<Guid>> Handle(AddPickCommand request, CancellationToken cancellationToken)
  {
    var draftId = DraftId.Create(request.DraftId);

    var draft = await _draftsRepository.GetDraftWithDetailsAsync(draftId, cancellationToken);

    if (draft is null)
    {
      return Result.Failure<Guid>(DraftErrors.NotFound(request.DraftId));
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

    var pick = Pick.Create(request.Position, movie, drafter, draft);

    if (pick.IsFailure)
    {
      return Result.Failure<Guid>(pick.Errors);
    }

    var result = draft.AddPick(pick.Value);

    if (result.IsFailure)
    {
      return Result.Failure<Guid>(result.Errors);
    }

    _picksRepository.Add(pick.Value);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(pick.Value.Id);
  }
}
