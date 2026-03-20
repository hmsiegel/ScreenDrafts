namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Participants.AddParticipantToDraftPart;

internal sealed class AddParticipantToDraftPartCommandHandler(
  IDraftPartRepository draftPartRepository,
  ParticipantResolver participantResolver)
  : ICommandHandler<AddParticipantToDraftPartCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly ParticipantResolver _participantResolver = participantResolver;

  public async Task<Result> Handle(AddParticipantToDraftPartCommand request, CancellationToken cancellationToken)
  {
    var draftPart = await _draftPartRepository.GetByPublicIdAsync(request.DraftPartId, cancellationToken);

    if (draftPart is null)
    {
      return Result.Failure(DraftPartErrors.NotFound(request.DraftPartId));
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

    var result = draftPart.AddParticipant(participant);

    if (result.IsFailure)
    {
      return Result.Failure(result.Errors);
    }

    _draftPartRepository.Update(draftPart);

    return result;
  }
}

