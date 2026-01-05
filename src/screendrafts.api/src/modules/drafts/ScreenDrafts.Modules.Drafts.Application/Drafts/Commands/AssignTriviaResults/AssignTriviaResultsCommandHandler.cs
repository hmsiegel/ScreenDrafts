namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AssignTriviaResults;

internal sealed class AssignTriviaResultsCommandHandler(
  IDraftersRepository draftersRepository,
  IDraftsRepository draftsRepository)
  : ICommandHandler<AssignTriviaResultsCommand>
{
  private readonly IDraftersRepository _draftersRepository = draftersRepository;
  private readonly IDraftsRepository _draftsRepository = draftsRepository;

  public async Task<Result> Handle(AssignTriviaResultsCommand request, CancellationToken cancellationToken)
  {
    var drafter = await _draftersRepository.GetByIdAsync(DrafterId.Create(request.DrafterId), cancellationToken);

    if (drafter is null)
    {
      return Result.Failure<TriviaResult>(DrafterErrors.NotFound(request.DrafterId));
    }

    var draftPartId = DraftPartId.Create(request.DraftPartId);

    var draftPart = await _draftsRepository.GetDraftPartByIdAsync(draftPartId, cancellationToken);

    if (draftPart is null)
    {
      return Result.Failure<TriviaResult>(DraftErrors.DraftPartNotFound(request.DraftPartId));
    }

    if (request.QuestionsWon < 0)
    {
      return Result.Failure<TriviaResult>(DrafterErrors.InvalidQuestionsWon);
    }

    if (request.Position < 0 || request.Position > draftPart.TotalDrafters)
    {
      return Result.Failure<TriviaResult>(DrafterErrors.InvalidPosition);
    }

    var result = draftPart.AddTriviaResult(drafter, null, request.Position, request.QuestionsWon);

    if (result.IsFailure)
    {
      return Result.Failure(result.Errors);
    }

    _draftsRepository.Update(draftPart.Draft);

    return Result.Success();
  }
}
