namespace ScreenDrafts.Common.Application.Seeding;

public interface ICustomSeeder
{
  Task InitializeAsync(CancellationToken cancellationToken = default);
}
