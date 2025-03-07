namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Commands.ExecuteVetoOverride;

internal sealed class ExecuteVetoOverrideCommandHandler(
  IDraftersRepository draftersRepository,
  IUnitOfWork unitOfWork,
  IVetoRepository vetoRepository)
  : ICommandHandler<ExecuteVetoOverrideCommand, Guid>
{
  private readonly IDraftersRepository _draftersRepository = draftersRepository;
  private readonly IVetoRepository _vetoRepository = vetoRepository;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;

  public async Task<Result<Guid>> Handle(ExecuteVetoOverrideCommand request, CancellationToken cancellationToken)
  {
    var drafterId = DrafterId.Create(request.DrafterId);

    var drafter = await _draftersRepository.GetByIdAsync(drafterId, cancellationToken);

    if (drafter is null)
    {
      return Result.Failure<Guid>(DrafterErrors.NotFound(request.DrafterId));
    }

    var veto = await _vetoRepository.GetByIdAsync(VetoId.Create(request.VetoId), cancellationToken);

    if (veto is null)
    {
      return Result.Failure<Guid>(VetoErrors.NotFound(request.VetoId));
    }

    var vetoOverride = await _vetoRepository.GetVetoOverrideByVetoIdAsync(veto.Id, cancellationToken);

    if (vetoOverride is not null)
    {
      return Result.Failure<Guid>(VetoErrors.VetoOverrideAlreadyUsed);
    }

    var vetoOverrideResult = VetoOverride.Create(veto);

    drafter.AddVetoOverride(vetoOverrideResult);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(vetoOverrideResult.Id.Value);
  }
}
