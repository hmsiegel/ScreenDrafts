namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Commands.AddDrafterToDrafterTeam;

internal sealed class AddDrafterToDrafterTeamCommandHandler(IDraftersRepository draftersRepository)
  : ICommandHandler<AddDrafterToDrafterTeamCommand>
{
  private readonly IDraftersRepository _draftersRepository = draftersRepository;

  public async Task<Result> Handle(AddDrafterToDrafterTeamCommand request, CancellationToken cancellationToken)
  {
    var drafterId = DrafterId.Create(request.DrafterId);
    var drafterTeamId = DrafterTeamId.Create(request.DrafterTeamId);

    var drafter = await _draftersRepository.GetByIdAsync(drafterId, cancellationToken);

    if (drafter is null)
    {
      return Result.Failure(DrafterErrors.NotFound(request.DrafterId));
    }

    var drafterTeam = await _draftersRepository.GetByIdAsync(drafterTeamId, cancellationToken);

    if (drafterTeam is null)
    {
      return Result.Failure(DrafterTeamErrors.NotFound(request.DrafterTeamId));
    }

    if (drafterTeam.Drafters.Count >= drafterTeam.NumberOfDrafters)
    {
      return Result.Failure(DrafterTeamErrors.TeamIsFull);
    }

    drafterTeam.AddDrafter(drafter);

    _draftersRepository.UpdateDrafterTeam(drafterTeam);
    return Result.Success();
  }
}
