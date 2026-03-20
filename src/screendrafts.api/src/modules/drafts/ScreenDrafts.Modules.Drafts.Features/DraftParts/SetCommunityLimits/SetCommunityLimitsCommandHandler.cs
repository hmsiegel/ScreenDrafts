namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SetCommunityLimits;

internal sealed class SetCommunityLimitsCommandHandler(IDraftPartRepository draftPartRepository) : ICommandHandler<SetCommunityLimitsCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;

  public async Task<Result> Handle(SetCommunityLimitsCommand request, CancellationToken cancellationToken)
  {
    var draftPart = await _draftPartRepository.GetByPublicIdAsync(request.DraftPartId, cancellationToken);

    if (draftPart is null)
    {
      return Result.Failure(DraftPartErrors.NotFound(request.DraftPartId));
    }

    var result = draftPart.SetCommunityLimits(
      maxCommunityPicks: request.MaxCommunityPicks,
      maxCommunityVetoes: request.MaxCommunityVetoes);

    if (result.IsFailure)
    {
      return Result.Failure(result.Errors);
    }

    _draftPartRepository.Update(draftPart);

    return Result.Success();
  }
}
