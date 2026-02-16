namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.Repositories;

public interface IDrafterRepository : IRepository<Drafter, DrafterId>
{
  void AddDrafterTeam(DrafterTeam drafterTeam);

  void UpdateDrafterTeam(DrafterTeam drafterTeam);

  Task<DrafterTeam?> GetByIdAsync(DrafterTeamId drafterTeamId, CancellationToken cancellationToken);

  Task<List<Drafter>> GetAll(CancellationToken cancellationToken = default);

  Task<bool> ExistsForPersonAsync(string personPublicId, CancellationToken cancellationToken);
}
