namespace ScreenDrafts.Modules.Drafts.Features.DrafterTeams.UpdateDrafterTeamName;

internal sealed class UpdateDrafterTeamNameCommandHandler(
  IDrafterTeamRepository drafterTeamRepository
) : ICommandHandler<UpdateDrafterTeamNameCommand>
{
  private readonly IDrafterTeamRepository _drafterTeamRepository = drafterTeamRepository;

  public async Task<Result> Handle(
    UpdateDrafterTeamNameCommand request,
    CancellationToken cancellationToken
  )
  {
    var team = await _drafterTeamRepository.GetByPublicIdAsync(
      request.DrafterTeamId,
      cancellationToken
    );

    if (team is null)
    {
      return Result.Failure(DrafterTeamErrors.NotFound(request.DrafterTeamId));
    }

    if (!string.Equals(team.Name, request.Name, StringComparison.Ordinal))
    {
      var exists = await _drafterTeamRepository.ExistsByNameAsync(request.Name, cancellationToken);

      if (exists)
      {
        return Result.Failure(DrafterTeamErrors.NameInUse(request.Name));
      }
    }

    var result = team.UpdateName(request.Name);

    if (result.IsFailure)
    {
      return result;
    }

    _drafterTeamRepository.Update(team);

    return Result.Success();
  }
}
