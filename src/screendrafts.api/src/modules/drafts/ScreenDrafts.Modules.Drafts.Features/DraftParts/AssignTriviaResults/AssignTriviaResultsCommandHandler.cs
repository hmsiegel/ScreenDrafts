namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.AssignTriviaResults;

internal sealed class AssignTriviaResultsCommandHandler(
  IDraftPartRepository draftPartRepository,
  ParticipantResolver participantResolver)
  : ICommandHandler<AssignTriviaResultsCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly ParticipantResolver _participantResolver = participantResolver;

  public async Task<Result> Handle(AssignTriviaResultsCommand request, CancellationToken cancellationToken)
  {
    var draftPart = await _draftPartRepository.GetByPublicIdAsync(request.DraftPartPublicId, cancellationToken);

    if (draftPart is null)
    {
      return Result.Failure(DraftPartErrors.NotFound(request.DraftPartPublicId));
    }

    var entries = new List<(Participant, int, int)>();

    foreach (var r in request.Results)
    {
      var participantResult = await _participantResolver.ResolveAsync(
        r.ParticipantPublicId,
        r.Kind,
        cancellationToken);

      if (participantResult.IsFailure)
      {
        return Result.Failure(participantResult.Errors);
      }

      var participant = participantResult.Value;

      var validationResult = participant.Validate();

      if (validationResult.IsFailure)
      {
        return Result.Failure(validationResult.Errors);
      }

      entries.Add((participant, r.Position, r.QuestionsWon));
    }

    var result = draftPart.AssignTriviaResults(entries);

    if (result.IsFailure)
    {
      return Result.Failure(result.Errors);
    }

    _draftPartRepository.Update(draftPart);

    return Result.Success();
  }
}

