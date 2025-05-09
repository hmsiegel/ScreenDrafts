namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Commands.ExecuteVetoOverride;

internal sealed class ExecuteVetoOverrideCommandHandler(
  IDraftersRepository draftersRepository,
  IVetoRepository vetoRepository)
  : ICommandHandler<ExecuteVetoOverrideCommand, Guid>
{
  private readonly IDraftersRepository _draftersRepository = draftersRepository;
  private readonly IVetoRepository _vetoRepository = vetoRepository;

  public async Task<Result<Guid>> Handle(ExecuteVetoOverrideCommand request, CancellationToken cancellationToken)
  {
    var hasDrafter = request.DrafterId.HasValue;
    var hasDrafterTeam = request.DrafterTeamId.HasValue;

    if (!BlessingValidation.IsValidBlessingRequest(request.DrafterId, request.DrafterTeamId))
    {
      return Result.Failure<Guid>(DrafterErrors.InvalidBlessingRequest);
    }

    var drafterId = hasDrafter
      ? DrafterId.Create(request.DrafterId!.Value)
      : null;

    var drafterTeamId = hasDrafterTeam
      ? DrafterTeamId.Create(request.DrafterTeamId!.Value)
      : null;

    var drafter = await _draftersRepository.GetByIdAsync(drafterId!, cancellationToken);
    var drafterTeam = await _draftersRepository.GetByIdAsync(drafterTeamId!, cancellationToken);

    if (drafter is null && hasDrafter)
    {
      return Result.Failure<Guid>(DrafterErrors.NotFound(drafterId!.Value));
    }

    if (drafterTeam is null && hasDrafterTeam)
    {
      return Result.Failure<Guid>(DrafterErrors.NotFound(drafterTeamId!.Value));
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

    var vetoOverrideResult = VetoOverride.Create(veto, drafter, drafterTeam).Value;

    if (hasDrafter && !hasDrafterTeam)
    {
      drafter!.AddVetoOverride(vetoOverrideResult);
    }
    else if (hasDrafterTeam && !hasDrafter)
    {
      drafterTeam!.AddVetoOverride(vetoOverrideResult);
    }
    else
    {
      return Result.Failure<Guid>(DrafterErrors.InvalidBlessingRequest);
    }

    return Result.Success(vetoOverrideResult.Id.Value);
  }
}
