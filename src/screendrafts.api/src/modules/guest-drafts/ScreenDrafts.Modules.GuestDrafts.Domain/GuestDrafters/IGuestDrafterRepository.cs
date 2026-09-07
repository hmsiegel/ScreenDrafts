namespace ScreenDrafts.Modules.GuestDrafts.Domain.GuestDrafters;

public interface IGuestDrafterRepository : IRepository<GuestDrafter, GuestDrafterId>
{
  Task<GuestDrafter?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
