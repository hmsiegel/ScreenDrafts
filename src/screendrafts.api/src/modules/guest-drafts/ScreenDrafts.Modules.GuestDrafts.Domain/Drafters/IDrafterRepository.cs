namespace ScreenDrafts.Modules.GuestDrafts.Domain.Drafters;

public interface IDrafterRepository : IRepository<Drafter, DrafterId>
{
  Task<Drafter?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
