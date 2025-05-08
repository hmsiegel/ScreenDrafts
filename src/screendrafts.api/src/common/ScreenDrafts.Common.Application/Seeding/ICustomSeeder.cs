namespace ScreenDrafts.Common.Application.Seeding;

public interface ICustomSeeder
{
  int Order { get; }
  string Name { get; }
  Task InitializeAsync(CancellationToken cancellationToken = default);
}
