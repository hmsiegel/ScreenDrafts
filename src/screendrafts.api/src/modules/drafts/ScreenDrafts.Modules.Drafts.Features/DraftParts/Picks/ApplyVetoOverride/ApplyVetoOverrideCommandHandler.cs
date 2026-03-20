namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.ApplyVetoOverride;

internal sealed class ApplyVetoOverrideCommandHandler(
  IDraftPartRepository draftPartRepository,
  ParticipantResolver participantResolver)
  : ICommandHandler<ApplyVetoOverrideCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly ParticipantResolver _participantResolver = participantResolver;

  public async Task<Result> Handle(ApplyVetoOverrideCommand request, CancellationToken cancellationToken)
  {
    var draftPart = await _draftPartRepository.GetByPublicIdAsync(request.DraftPartId, cancellationToken);

    if (draftPart is null)
    {
      return Result.Failure(DraftPartErrors.NotFound(request.DraftPartId));
    }

    var participantResult = await _participantResolver.ResolveAsync(
      request.ParticipantIdValue,
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

    var result = draftPart.ApplyVetoOverride(
      request.PlayOrder,
      by: participant,
      actedByPublicId: request.ActorPublicId);

    if (result.IsFailure)
    {
      return Result.Failure(result.Errors);
    }

    _draftPartRepository.Update(draftPart);

    return Result.Success();
  }
}
