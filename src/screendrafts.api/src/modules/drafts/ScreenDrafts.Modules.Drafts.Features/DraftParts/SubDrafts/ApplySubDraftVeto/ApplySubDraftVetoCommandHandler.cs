namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.ApplySubDraftVeto;

internal sealed class ApplySubDraftVetoCommandHandler(
  IDraftPartRepository draftPartRepository,
  ParticipantResolver participantResolver)
  : ICommandHandler<ApplySubDraftVetoCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly ParticipantResolver _participantResolver = participantResolver;

  public async Task<Result> Handle(ApplySubDraftVetoCommand request, CancellationToken cancellationToken)
  {
    var draftPart = await _draftPartRepository.GetByPublicIdWithSubDraftsAsync(request.DraftPartPublicId, cancellationToken);

    if (draftPart is null)
      return Result.Failure(DraftPartErrors.NotFound(request.DraftPartPublicId));

    var subDraft = draftPart.SubDrafts
      .FirstOrDefault(x => x.PublicId == request.SubDraftPublicId);

    if (subDraft is null)
      return Result.Failure(SubDraftErrors.NotFound(request.SubDraftPublicId));

    if (subDraft.Status != SubDraftStatus.Active)
      return Result.Failure(SubDraftErrors.MustBeActive);

    var pick = draftPart.Picks
      .FirstOrDefault(x => x.PlayOrder == request.PlayOrder);

    if (pick is null)
      return Result.Failure(DraftPartErrors.PickNotFound(request.PlayOrder));

    var issuerResult = await _participantResolver.ResolveAsync(
      request.IssuerPublicId,
      request.IssuerKind,
      cancellationToken);

    if (issuerResult.IsFailure)
      return Result.Failure(issuerResult.Errors);

    var issuer = issuerResult.Value;

    var validationResult = issuer.Validate();

    if (validationResult.IsFailure)
      return Result.Failure(validationResult.Errors);

    var result = draftPart.ApplyVeto(
      pickId: pick.Id,
      issuerId: issuer,
      actedByPublicId: request.ActedbyPublicId);

    if (result.IsFailure)
      return Result.Failure(result.Errors);

    _draftPartRepository.Update(draftPart);

    return Result.Success();
  }
}
