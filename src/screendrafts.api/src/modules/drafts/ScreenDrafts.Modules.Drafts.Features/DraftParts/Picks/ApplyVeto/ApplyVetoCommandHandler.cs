namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.ApplyVeto;

internal sealed class ApplyVetoCommandHandler(
  IDraftPartRepository draftPartRepository,
  ParticipantResolver participantResolver,
  IPickRepository pickRepository)
  : ICommandHandler<ApplyVetoCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly ParticipantResolver _participantResolver = participantResolver;
  private readonly IPickRepository _pickRepository = pickRepository;

  public async Task<Result> Handle(ApplyVetoCommand request, CancellationToken cancellationToken)
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

    participant.Validate();

    var pick = await _pickRepository.GetByDraftPartIdAndPlayOrderAsync(draftPart.Id, request.PlayOrder, cancellationToken);

    if (pick is null)
    {
      return Result.Failure(DraftPartErrors.PickNotFound(request.PlayOrder));
    }

    var pickId = PickId.Create(pick.Id.Value);

    var pickResult = draftPart.ApplyVeto(
      pickId: pickId,
      issuerId: participant,
      actedByPublicId: request.ActorPublicId);

    if (pickResult.IsFailure)
    {
      return Result.Failure(pickResult.Errors);
    }

    _draftPartRepository.Update(draftPart);

    return Result.Success();
  }
}
