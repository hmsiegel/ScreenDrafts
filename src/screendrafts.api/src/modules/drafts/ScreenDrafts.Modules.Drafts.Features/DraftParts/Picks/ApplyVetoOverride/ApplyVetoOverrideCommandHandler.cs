namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.ApplyVetoOverride;

internal sealed class ApplyVetoOverrideCommandHandler(
  IDraftPartRepository draftPartRepository,
  IPickRepository pickRepository,
  ParticipantResolver participantResolver,
  IVetoRepository vetoRepository)
  : ICommandHandler<ApplyVetoOverrideCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly IPickRepository _pickRepository = pickRepository;
  private readonly ParticipantResolver _participantResolver = participantResolver;
  private readonly IVetoRepository _vetoRepository = vetoRepository;

  public async Task<Result> Handle(ApplyVetoOverrideCommand request, CancellationToken cancellationToken)
  {
    var draftPart = await _draftPartRepository.GetByPublicIdAsync(request.DraftPartId, cancellationToken);

    if (draftPart is null)
    {
      return Result.Failure(DraftPartErrors.NotFound(request.DraftPartId));
    }

    var pick = await _pickRepository.GetByDraftPartIdAndPlayOrderAsync(
      draftPart.Id,
      request.PlayOrder,
      cancellationToken);

    if (pick is null)
    {
      return Result.Failure(DraftPartErrors.PickNotFound(request.PlayOrder));
    }

    var veto = pick.Veto;

    if (veto is null)
    {
      return Result.Failure(DraftPartErrors.VetoNotFound(request.PlayOrder));
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

    var overrideResult = veto.Override(participant, request.ActorPublicId);

    if (overrideResult.IsFailure)
    {
      return Result.Failure(overrideResult.Errors);
    }

    _pickRepository.Update(pick);
    _vetoRepository.UpdateVeto(veto);

    return Result.Success();
  }
}
