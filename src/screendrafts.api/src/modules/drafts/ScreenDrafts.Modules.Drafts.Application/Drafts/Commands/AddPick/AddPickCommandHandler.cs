namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddPick;

internal sealed class AddPickCommandHandler(
  IDraftsRepository draftsRepository,
  IPicksRepository picksRepository)
  : ICommandHandler<AddPickCommand, Guid>
{
  private readonly IDraftsRepository _draftsRepository = draftsRepository;
  private readonly IPicksRepository _picksRepository = picksRepository;

  public async Task<Result<Guid>> Handle(AddPickCommand request, CancellationToken cancellationToken)
  {
    var draftId = DraftId.Create(request.DraftId);

    var draft = await _draftsRepository.GetDraftWithDetailsAsync(draftId, cancellationToken);

    if (draft is null)
    {
      return Result.Failure<Guid>(DraftErrors.NotFound(request.DraftId));
    }

    var drafter = draft.Drafters.FirstOrDefault(d => d.Id.Value == request.DrafterId);
    var drafterTeam = draft.DrafterTeams.FirstOrDefault(dt => dt.Id.Value == request.DrafterId);

    if (drafter is null && drafterTeam is null)
    {
      return Result.Failure<Guid>(DrafterErrors.NotFound(request.DrafterId, request.DrafterTeamId));
    }

    var movie = await _draftsRepository.GetMovieByIdAsync(request.MovieId, cancellationToken);

    if (movie is null)
    {
      return Result.Failure<Guid>(DraftErrors.MovieNotFound(request.MovieId));
    }

    var pick = Pick.Create(request.Position, movie, drafter, drafterTeam, draft, request.PlayOrder);

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

    return Result.Success(pick.Value.Id.Value);
  }
}
