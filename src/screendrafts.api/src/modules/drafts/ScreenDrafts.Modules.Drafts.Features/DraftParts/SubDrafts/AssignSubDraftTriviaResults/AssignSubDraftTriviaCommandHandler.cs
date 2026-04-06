namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.AssignSubDraftTriviaResults;

internal sealed record AssignSubDraftTriviaCommandHandler(
  IDraftPartRepository draftPartRepository,
  ParticipantResolver participantResolver)
  : ICommandHandler<AssignSubDraftTriviaCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly ParticipantResolver _participantResolver = participantResolver;

  public async Task<Result> Handle(AssignSubDraftTriviaCommand request, CancellationToken cancellationToken)
  {
    var draftPart = await _draftPartRepository.GetByPublicIdWithSubDraftsAsync(request.DraftPartPublicId, cancellationToken);

    if (draftPart is null)
    {
      return Result.Failure(DraftPartErrors.NotFound(request.DraftPartPublicId));
    }

    var subDraft = draftPart.SubDrafts
      .FirstOrDefault(x => x.PublicId == request.SubDraftPublicId);

    if (subDraft is null)
    {
      return Result.Failure(SubDraftErrors.NotFound(request.SubDraftPublicId));
    }

    if (subDraft.Status != SubDraftStatus.Pending)
    {
      return Result.Failure(SubDraftErrors.CannotActivateSubDraft);
    }

    var entries = new List<(Participant, int, int)>();

    foreach (var r in request.Results)
    {
      var participantResult = await _participantResolver.ResolveAsync(
        r.ParticipantPublicId,
        ParticipantKind.FromValue(r.Kind),
        cancellationToken);

      if (participantResult.IsFailure)
      {
        return Result.Failure(participantResult.Error!);
      }

      var participant = participantResult.Value;

      var validationResult = participant.Validate();

      if (validationResult.IsFailure)
      {
        return Result.Failure(validationResult.Errors);
      }

      entries.Add((participant, r.Position, r.QuestionsWon));
    }

    var triviaResult = draftPart.AssignSubDraftTriviaResults(subDraft.Id, entries);

    if (triviaResult.IsFailure)
    {
      return triviaResult;
    }

    var activateResult = subDraft.Activate();

    if (activateResult.IsFailure)
    {
      return activateResult;
    }

    _draftPartRepository.Update(draftPart);

    return Result.Success();
  }
}
