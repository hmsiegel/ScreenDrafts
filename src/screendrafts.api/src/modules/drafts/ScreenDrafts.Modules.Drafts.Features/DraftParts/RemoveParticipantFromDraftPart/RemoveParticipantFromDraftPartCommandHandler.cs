namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.RemoveParticipantFromDraftPart;

internal sealed class RemoveParticipantFromDraftPartCommandHandler : ICommandHandler<RemoveParticipantFromDraftPartCommand>
{
  private readonly IDraftPartRepository _draftPartRepository;
  private readonly ParticipantResolver _participantResolver;

  public RemoveParticipantFromDraftPartCommandHandler(IDraftPartRepository draftPartRepository, ParticipantResolver participantResolver)
  {
    _draftPartRepository = draftPartRepository;
    _participantResolver = participantResolver;
  }

  public async Task<Result> Handle(RemoveParticipantFromDraftPartCommand request, CancellationToken cancellationToken)
  {
    var draftPart = await _draftPartRepository.GetByPublicIdAsync(request.DraftPartPublicId, cancellationToken);

    if (draftPart is null)
    {
      return Result.Failure(DraftPartErrors.NotFound(request.DraftPartPublicId));
    }

    var participantResult = await _participantResolver.ResolveAsync(
      request.ParticipantPublicId,
      request.ParticipantKind,
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

    var result = draftPart.RemoveParticipant(participant);

    if (result.IsFailure)
    {
      return Result.Failure(result.Errors);
    }

    _draftPartRepository.Update(draftPart);

    return result;
  }
}
