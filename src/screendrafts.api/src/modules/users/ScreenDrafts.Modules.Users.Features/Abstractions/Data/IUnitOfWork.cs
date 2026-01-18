namespace ScreenDrafts.Modules.Users.Features.Abstractions.Data;

public interface IUnitOfWork
{
  Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
