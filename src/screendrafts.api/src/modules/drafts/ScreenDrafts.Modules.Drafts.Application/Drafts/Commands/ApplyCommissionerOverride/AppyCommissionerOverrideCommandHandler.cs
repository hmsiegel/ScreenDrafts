namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.ApplyCommissionerOverride;

internal sealed class AppyCommissionerOverrideCommandHandler(
  IDraftsRepository draftRepository,
  IPicksRepository picksRepository)
  : ICommandHandler<ApplyCommissionerOverrideCommand, Guid>
{
  private readonly IDraftsRepository _draftRepository = draftRepository;
  private readonly IPicksRepository _picksRepository = picksRepository;

  public async Task<Result<Guid>> Handle(ApplyCommissionerOverrideCommand request, CancellationToken cancellationToken)
  {
    var pickId = PickId.Create(request.PickId);
    var pick = await _picksRepository.GetByIdAsync(pickId, cancellationToken);

    if (pick is null)
    {
      return Result.Failure<Guid>(PickErrors.NotFound(request.PickId));
    }

    var commissionerOverride = CommissionerOverride.Create(pick).Value;

    pick.ApplyCommissionerOverride(commissionerOverride);

    _draftRepository.AddCommissionerOverride(commissionerOverride);
    _picksRepository.Update(pick);

    return Result.Success(pick.Id.Value);
  }
}
