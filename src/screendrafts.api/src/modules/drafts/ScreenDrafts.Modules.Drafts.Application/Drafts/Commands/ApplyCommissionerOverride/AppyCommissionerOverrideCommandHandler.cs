
namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.ApplyCommissionerOverride;

internal sealed class AppyCommissionerOverrideCommandHandler(
  IDraftsRepository draftRepository,
  IUnitOfWork unitOfWork,
  IPicksRepository picksRepository)
  : ICommandHandler<ApplyCommissionerOverrideCommand, Guid>
{
  private readonly IDraftsRepository _draftRepository = draftRepository;
  private readonly IPicksRepository _picksRepository = picksRepository;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;

  public async Task<Result<Guid>> Handle(ApplyCommissionerOverrideCommand request, CancellationToken cancellationToken)
  {
    var pickId = PickId.Create(request.PickId);
    var pick = await _picksRepository.GetByIdAsync(pickId, cancellationToken);

    if (pick is null)
    {
      return Result.Failure<Guid>(PickErrors.NotFound(request.PickId));
    }

    var draft = await _draftRepository.GetByIdAsync(pick.DraftId, cancellationToken);

    if (draft is null)
    {
      return Result.Failure<Guid>(DraftErrors.NotFound(pick.DraftId.Value));
    }

    var commissionerOverride = CommissionerOverride.Create(pick).Value;

    draft.ApplyCommissionerOverride(commissionerOverride);

    _draftRepository.AddCommissionerOverride(commissionerOverride);
    _draftRepository.Update(draft);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(pick.Id.Value);
  }
}
