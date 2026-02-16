using ScreenDrafts.Modules.Drafts.Domain.DraftParts.Entities;
using ScreenDrafts.Modules.Drafts.Domain.DraftParts.Repositories;
using ScreenDrafts.Modules.Drafts.Domain.DraftParts.ValueObjects;

namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafters.Commands.ExecuteVeto;

internal sealed class ExecuteVetoCommandHandler(
  IDraftersRepository draftersRepository,
  IPicksRepository picksRepository,
  IDraftRepository draftsRepository,
  IVetoRepository vetoRepository)
  : ICommandHandler<ExecuteVetoCommand, Guid>
{
  private readonly IDraftersRepository _draftersRepository = draftersRepository;
  private readonly IDraftRepository _draftsRepository = draftsRepository;
  private readonly IPicksRepository _picksRepository = picksRepository;
  private readonly IVetoRepository _vetoRepository = vetoRepository;

  public async Task<Result<Guid>> Handle(ExecuteVetoCommand request, CancellationToken cancellationToken)
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

    var pickId = PickId.Create(request.PickId);

    var pick = await _picksRepository.GetByIdAsync(pickId, cancellationToken);

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

    var veto = await _vetoRepository.GetByPickAsync(PickId.Create(request.PickId), cancellationToken);

    if (veto is not null)
    {
      return Result.Failure<Guid>(VetoErrors.VetoAlreadyUsed);
    }

    var result = Veto.Create(pick, drafter, drafterTeam);

    if (result.IsFailure)
    {
      return Result.Failure<Guid>(result.Errors);
    }

    if (hasDrafter && !hasDrafterTeam)
    {
      drafter!.AddVeto(result.Value);
    }
    else if (hasDrafterTeam && !hasDrafter)
    {
      drafterTeam!.AddVeto(result.Value);
    }
    else
    {
      return Result.Failure<Guid>(DrafterErrors.InvalidBlessingRequest);
    }

    _draftersRepository.Update(drafter!);

    return Result.Success(result.Value.Id.Value);
  }
}
