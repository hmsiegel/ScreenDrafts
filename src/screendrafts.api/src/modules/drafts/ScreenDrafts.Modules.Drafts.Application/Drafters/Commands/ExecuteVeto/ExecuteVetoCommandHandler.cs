namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Commands.ExecuteVeto;

internal sealed class ExecuteVetoCommandHandler(
  IDraftersRepository draftersRepository,
  IUnitOfWork unitOfWork,
  IPicksRepository picksRepository) : ICommandHandler<ExecuteVetoCommand>
{
  private readonly IDraftersRepository _draftersRepository = draftersRepository;
  private readonly IPicksRepository _picksRepository = picksRepository;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;

  public async Task<Result> Handle(ExecuteVetoCommand request, CancellationToken cancellationToken)
  {
    var drafterId = DrafterId.Create(request.DrafterId);

    var drafter = await _draftersRepository.GetByIdAsync(drafterId, cancellationToken);

    if (drafter is null)
    {
      return Result.Failure<Drafter>(DrafterErrors.NotFound(request.DrafterId));
    }

    var pick = await _picksRepository.GetByIdAsync(request.PickId, cancellationToken);

    if (pick is null)
    {
      return Result.Failure<Drafter>(PickErrors.NotFound(request.PickId));
    }

    drafter.AddVeto(pick);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success();
  }
}
