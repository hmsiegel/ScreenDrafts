namespace ScreenDrafts.Modules.Drafts.Domain.DrafterTeams;

public interface IDrafterTeamRepository : IRepository<DrafterTeam, DrafterTeamId>
{
  Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken);
}
