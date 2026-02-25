namespace ScreenDrafts.Modules.Drafts.Features.DrafterTeams.AddDrafterToTeam;

internal sealed class AddDrafterToTeamCommandHandler : ICommandHandler<AddDrafterToTeamCommand>
{
  private readonly IDrafterTeamRepository _drafterTeamRepository;
  private readonly IDrafterRepository _drafterRepository;

  public AddDrafterToTeamCommandHandler(IDrafterTeamRepository drafterTeamRepository, IDrafterRepository drafterRepository)
  {
    _drafterTeamRepository = drafterTeamRepository;
    _drafterRepository = drafterRepository;
  }

  public async Task<Result> Handle(AddDrafterToTeamCommand request, CancellationToken cancellationToken)
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

    team.AddDrafter(drafter);
    _drafterTeamRepository.Update(team);

    return Result.Success();
  }
}
