using ScreenDrafts.Modules.Drafts.Domain.Drafters.Entities;

namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Commands.ExecuteVeto;

internal sealed class ExecuteVetoCommandHandler(
  IDraftersRepository draftersRepository,
  IUnitOfWork unitOfWork,
  IPicksRepository picksRepository,
  IDraftsRepository draftsRepository,
  IVetoRepository vetoRepository)
  : ICommandHandler<ExecuteVetoCommand, Guid>
{
  private readonly IDraftersRepository _draftersRepository = draftersRepository;
  private readonly IDraftsRepository _draftsRepository = draftsRepository;
  private readonly IPicksRepository _picksRepository = picksRepository;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;
  private readonly IVetoRepository _vetoRepository = vetoRepository;

  public async Task<Result<Guid>> Handle(ExecuteVetoCommand request, CancellationToken cancellationToken)
  {
    var drafterId = DrafterId.Create(request.DrafterId);

    var drafter = await _draftersRepository.GetByIdAsync(drafterId, cancellationToken);

    if (drafter is null)
    {
      return Result.Failure<Guid>(DrafterErrors.NotFound(request.DrafterId));
    }

    var pick = await _picksRepository.GetByIdAsync(request.PickId, cancellationToken);

    if (pick is null)
    {
      return Result.Failure<Guid>(PickErrors.NotFound(request.PickId));
    }

    var draftId = DraftId.Create(request.DraftId);

    var draft = await _draftsRepository.GetByIdAsync(draftId, cancellationToken);

    if (draft is null)
    {
      return Result.Failure<Guid>(DraftErrors.NotFound(request.DraftId));
    }

    if (draft.DraftStatus != DraftStatus.InProgress)
    {
      return Result.Failure<Guid>(DraftErrors.CannotVetoUnlessTheDraftIsStarted);
    }

    var veto = await _vetoRepository.GetByPickAsync(request.PickId, cancellationToken);

    if (veto is not null)
    {
      return Result.Failure<Guid>(VetoErrors.VetoAlreadyUsed);
    }

    var result = Veto.Create(pick);

    if (result.IsFailure)
    {
      return Result.Failure<Guid>(result.Errors);
    }

    drafter.AddVeto(result.Value);

    _draftersRepository.Update(drafter);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(result.Value.Id.Value);
  }
}
