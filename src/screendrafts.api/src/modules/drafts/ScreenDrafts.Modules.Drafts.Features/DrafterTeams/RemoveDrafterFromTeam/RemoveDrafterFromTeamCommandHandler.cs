namespace ScreenDrafts.Modules.Drafts.Features.DrafterTeams.RemoveDrafterFromTeam;

internal sealed class RemoveDrafterFromTeamCommandHandler : ICommandHandler<RemoveDrafterFromTeamCommand>
{
  private readonly IDrafterTeamRepository _drafterTeamRepository;
  private readonly IDrafterRepository _drafterRepository;

  public RemoveDrafterFromTeamCommandHandler(IDrafterTeamRepository drafterTeamRepository, IDrafterRepository drafterRepository)
  {
    _drafterTeamRepository = drafterTeamRepository;
    _drafterRepository = drafterRepository;
  }

  public async Task<Result> Handle(RemoveDrafterFromTeamCommand request, CancellationToken cancellationToken)
  {
    var team = await _drafterTeamRepository.GetByPublicIdAsync(request.DrafterTeamId, cancellationToken);

    if (team is null)
    {
      return Result.Failure(DrafterTeamErrors.NotFound(request.DrafterTeamId));
    }

    var drafter = await _drafterRepository.GetByPublicIdAsync(request.DrafterId, cancellationToken);

    if (drafter is null)
    {
      return Result.Failure(DrafterErrors.NotFound(request.DrafterId));
    }

    team.RemoveDrafter(drafter);

    _drafterTeamRepository.Update(team);

    return Result.Success();
  }
}
