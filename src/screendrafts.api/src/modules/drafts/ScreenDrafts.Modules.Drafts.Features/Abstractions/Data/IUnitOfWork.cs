namespace ScreenDrafts.Modules.Drafts.Features.Abstractions.Data;

public interface IUnitOfWork
{
  Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
