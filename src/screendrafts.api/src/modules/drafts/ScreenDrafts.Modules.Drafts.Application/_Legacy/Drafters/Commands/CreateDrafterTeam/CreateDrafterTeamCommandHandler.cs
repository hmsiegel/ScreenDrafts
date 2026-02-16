namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafters.Commands.CreateDrafterTeam;

internal sealed class CreateDrafterTeamCommandHandler(IDraftersRepository draftersRepository)
  : ICommandHandler<CreateDrafterTeamCommand, Guid>
{
  private readonly IDraftersRepository _draftersRepository = draftersRepository;

  public Task<Result<Guid>> Handle(CreateDrafterTeamCommand request, CancellationToken cancellationToken)
  {
    var drafterTeam = DrafterTeam.Create(request.TeamName).Value;

    _draftersRepository.AddDrafterTeam(drafterTeam);

    return Task.FromResult(Result.Success(drafterTeam.Id.Value));
  }
}
