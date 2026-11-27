namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Participants.SetPhantomParticipant;

internal sealed class SetPhantomParticipantCommandHandler(
  IDraftPartRepository draftPartRepository,
  ParticipantResolver participantResolver
) : ICommandHandler<SetPhantomParticipantCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly ParticipantResolver _participantResolver = participantResolver;

  public async Task<Result> Handle(
    SetPhantomParticipantCommand request,
    CancellationToken cancellationToken
  )
  {
    var draftPart = await _draftPartRepository.GetByPublicIdAsync(
      request.DraftPartId,
      cancellationToken
    );

    if (draftPart is null)
    {
      return Result.Failure(DraftPartErrors.NotFound(request.DraftPartId));
    }

    // Only drafters can be phantoms, so the kind is fixed here instead of coming from the caller.
    var participantResult = await _participantResolver.ResolveAsync(
      request.ParticipantPublicId,
      ParticipantKind.Drafter,
      cancellationToken
    );

    if (participantResult.IsFailure)
    {
      return Result.Failure(participantResult.Errors);
    }

    var result = draftPart.SetPhantomParticipant(participantResult.Value, request.IsPhantom);

    if (result.IsFailure)
    {
      return result;
    }

    _draftPartRepository.Update(draftPart);

    return result;
  }
}
