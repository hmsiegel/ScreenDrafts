namespace ScreenDrafts.Modules.GuestDrafts.Domain.Abstractions.Data;

public interface IUnitOfWork
{
  Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
