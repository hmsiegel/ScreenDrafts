namespace ScreenDrafts.Modules.Drafts.Application.Abstractions.Data;

public interface IUnitOfWork
{
  Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
